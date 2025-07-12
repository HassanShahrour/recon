using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Data.Models;

namespace Reconova.Controllers
{
    public class UserController : Controller
    {

        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userRepository.GetAllUsers();
                return View(users.Value ?? new List<User>());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching users: " + ex.Message);
                return View(new List<User>());
            }
        }


        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var user = await _userRepository.GetUserById(id);
                return View(user.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching user: " + ex.Message);
                return View(new User());
            }
        }


        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {

                var result = await _userRepository.UpdateUser(user);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = $"User updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to update user: {result.Error}";
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(Index));


            }
            catch (Exception ex)
            {
                Console.WriteLine("Controller error updating user: " + ex.Message);
                return StatusCode(500, "An error occurred while updating the user.");
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _userRepository.DeleteUser(id);
                if (result.IsSuccess)
                    TempData["Success"] = "User deleted successfully.";
                else
                    TempData["Error"] = "Error while deleting user";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
