using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Core.Utilities;
using Reconova.Data;
using Reconova.Data.DTOs.User;
using Reconova.Data.Models;
using Reconova.Hubs;
using Reconova.ViewModels.Plans;

namespace Reconova.Controllers
{

    [Authorize]
    public class PlanController : Controller
    {
        private readonly ReconovaDbContext _context;
        private readonly IPlanRepository _planRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserUtility _userUtility;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PlanController(IPlanRepository planRepository,
            IUserRepository userRepository,
            UserUtility userUtility,
            ReconovaDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            _planRepository = planRepository;
            _userRepository = userRepository;
            _userUtility = userUtility;
            _context = context;
            _hubContext = hubContext;
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var plans = await _planRepository.GetAllPlans();

                return View(plans.Value ?? new List<Plan>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }


        public async Task<IActionResult> Offers()
        {
            try
            {
                var plans = await _planRepository.GetAllPlans();
                var userId = await _userUtility.GetLoggedInUserId();
                var user = await _userRepository.GetUserById(userId.ToString());

                var model = new PlansViewModel
                {
                    Plans = plans.Value ?? new List<Plan>(),
                    User = user.Value ?? new User()
                };

                return View(model ?? new PlansViewModel());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("api/plan")]
        public async Task<IActionResult> CreatePlanApi([FromBody] Plan plan)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _planRepository.AddPlan(plan);
            if (result.IsSuccess)
                return Ok(plan);

            return StatusCode(500, "Failed to add plan");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        [Route("api/plan/{id}")]
        public async Task<IActionResult> EditPlanApi(int id, [FromBody] Plan updatedPlan)
        {
            var existing = await _planRepository.GetPlanById(id);
            if (!existing.IsSuccess || existing.Value == null)
                return NotFound("Plan not found");

            updatedPlan.Id = id;

            var result = await _planRepository.UpdatePlan(updatedPlan);
            if (result.IsSuccess)
                return Ok(updatedPlan);

            return StatusCode(500, "Failed to update plan");
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Route("api/plan/{id}")]
        public async Task<IActionResult> DeletePlanApi(int id)
        {
            var result = await _planRepository.DeletePlan(id);
            if (result.IsSuccess)
                return NoContent();

            return StatusCode(500, "Failed to delete plan");
        }


        [HttpPost]
        [Route("api/plan/RequestSubscription")]
        public async Task<IActionResult> RequestSubscription([FromBody] RequestPlanDTO dto)
        {
            var userId = await _userUtility.GetLoggedInUserId();
            var user = await _userRepository.GetUserById(userId.ToString());

            if (user == null)
                return BadRequest(new { success = false, message = "User not found." });

            var plan = await _context.Plan.FindAsync(dto.PlanId);
            if (plan == null)
                return BadRequest(new { success = false, message = "Plan not found." });

            var admins = await _context.Users
                .Where(u => u.Role == "Admin")
                .ToListAsync();

            foreach (var admin in admins)
            {
                var notification = new Notification
                {
                    SenderId = userId.ToString(),
                    ReceiverId = admin.Id,
                    Message = $"{user.Value.FirstName} has requested to upgrade to the {plan.Name} plan.",
                    Type = "PlanRequest",
                    CreatedDate = DateTime.UtcNow
                };
                _context.Notification.Add(notification);

                await _hubContext.Clients.User(admin.Id).SendAsync("ReceiveNotification", new
                {
                    Type = "PlanRequest",
                    Message = $"{user.Value.FirstName} has requested to upgrade to the {plan.Name} plan.",
                    SenderName = user.Value.FirstName,
                    PlanId = plan.Id
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Your request was sent to the administrators." });
        }



    }
}
