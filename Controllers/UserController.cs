using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Data;
using Reconova.Data.DTOs.User;
using Reconova.Data.Models;
using Reconova.Hubs;
using Reconova.ViewModels.Users;

namespace Reconova.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPlanRepository _planRepository;
        private readonly ReconovaDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public UserController(
            IUserRepository userRepository,
            IPlanRepository planRepository,
            ReconovaDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            _userRepository = userRepository;
            _planRepository = planRepository;
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userRepository.GetAllUsers();
                var plans = await _planRepository.GetAllPlans();

                if (!users.IsSuccess || !plans.IsSuccess)
                {
                    TempData["Error"] = "Failed to load users or plans.";
                    return View(new UsersViewModel());
                }

                var model = new UsersViewModel
                {
                    Users = users.Value ?? new List<User>(),
                    Plans = plans.Value ?? new List<Plan>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error loading users: {ex.Message}";
                return View(new UsersViewModel());
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var user = await _userRepository.GetUserById(id);

                if (!user.IsSuccess || user.Value == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user.Value);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error fetching user: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            try
            {
                var result = await _userRepository.UpdateUser(user);

                if (result.IsSuccess)
                {
                    TempData["Success"] = "User updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = $"Failed to update user: {result.Error}";
                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return View(user);
            }
        }

        [HttpGet("/User/Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _userRepository.DeleteUser(id);
                TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
                    ? "User status updated successfully."
                    : result.Error ?? "Failed to update user status.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("api/users/assign-plan")]
        public async Task<IActionResult> AssignPlanToUser([FromBody] AssignPlanDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data.");

            try
            {
                var result = await _planRepository.AssignPlanToUserAsync(dto.UserId ?? "", dto.PlanId);

                if (!result.IsSuccess)
                    return BadRequest(result.Error);

                var user = await _userRepository.GetUserById(dto.UserId ?? "");
                if (user.Value == null)
                    return NotFound("User not found.");

                var plan = await _context.Plan.FindAsync(dto.PlanId);
                var planName = plan?.Name ?? "a new plan";

                var notification = new Notification
                {
                    SenderId = null,
                    ReceiverId = user.Value.Id,
                    Message = $"Your plan has been upgraded to {planName}.",
                    Type = "PlanUpgrade",
                    CreatedDate = DateTime.UtcNow
                };

                _context.Notification.Add(notification);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.User(user.Value.Id).SendAsync("ReceiveNotification", new
                {
                    Type = "PlanUpgrade",
                    Message = $"Your plan has been upgraded to {planName}.",
                    PlanId = dto.PlanId
                });

                return Ok("Plan assigned successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error assigning plan: {ex.Message}");
            }
        }
    }
}
