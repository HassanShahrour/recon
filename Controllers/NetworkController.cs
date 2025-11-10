using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Data.Models;
using Reconova.ViewModels.Users;

namespace Reconova.Controllers
{

    [Authorize]
    public class NetworkController : Controller
    {

        private readonly IUserRepository _userRepository;

        public NetworkController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userRepository.GetAllUsersExceptLoggedIn();
                return View(users.Value ?? new List<User>());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching users: " + ex.Message);
                return View(new List<User>());
            }
        }

        public async Task<IActionResult> ViewUser(string id)
        {
            try
            {
                var user = await _userRepository.GetUserById(id);
                var posts = await _userRepository.GetUserPosts(user.Value.Id);
                var model = new ProfileViewModel
                {
                    User = user.Value ?? new User(),
                    Posts = posts.Value ?? new List<Post>(),
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching user: " + ex.Message);
                return View(new ProfileViewModel());
            }
        }


    }
}
