using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Data.Models;

namespace Reconova.Controllers
{
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
                var users = await _userRepository.GetAllUsers();
                return View(users.Value ?? new List<User>());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching users: " + ex.Message);
                return View(new List<User>());
            }
        }


    }
}
