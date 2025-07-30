using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Core.Utilities;
using Reconova.Data.DTOs.User;
using Reconova.Data.Models;
using Reconova.ViewModels.Users;

namespace Reconova.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly UserUtility _userUtility;

        public AccountController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IUserRepository userRepository,
            UserUtility userUtility)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userRepository = userRepository;
            _userUtility = userUtility;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError(string.Empty, "Invalid credentials!");
            return View(model);
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                BirthDate = model.BirthDate,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                UserName = model.Email,
                Role = "User"
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        public IActionResult VerifyEmail() => View();

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Something is wrong!");
                return View(model);
            }

            return RedirectToAction("ChangePassword", new { username = user.UserName });
        }

        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Home");

            return View(new ChangePasswordDTO { Email = username });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Something went wrong. Try again.");
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? "");

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email not found!");
                return View(model);
            }

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            var addResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (addResult.Succeeded)
                return RedirectToAction("Login");

            foreach (var error in addResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile()
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();
                var user = await _userRepository.GetUserById(userId.ToString());
                var posts = await _userRepository.GetUserPosts(userId.ToString());

                return View(new ProfileViewModel
                {
                    User = user.Value ?? new User(),
                    Posts = posts.Value ?? new List<Post>()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Fetching profile: {ex.Message}");
                return View(new ProfileViewModel());
            }
        }

        public async Task<IActionResult> Edit()
        {
            try
            {
                var id = await _userUtility.GetLoggedInUserId();
                var userRequest = await _userRepository.GetUserById(id.ToString());
                var user = userRequest.Value;

                if (user == null)
                    return NotFound();

                return View(new EditUserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    BirthDate = user.BirthDate,
                    Education = user.Education,
                    Header = user.Header,
                    Bio = user.Bio,
                    Country = user.Country,
                    ProfilePhotoPath = user.ProfilePhotoPath,
                    CoverPhotoPath = user.CoverPhotoPath,
                    Skills = user.Skills?.ToList() ?? new()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Loading Edit User: {ex.Message}");
                return View(new EditUserViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var id = await _userUtility.GetLoggedInUserId();
                var userResult = await _userRepository.GetUserById(id.ToString());

                if (!userResult.IsSuccess || userResult.Value == null)
                    return NotFound();

                var user = userResult.Value;

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email?.Trim();
                user.UserName = user.Email;
                user.NormalizedEmail = user.Email?.ToUpper();
                user.NormalizedUserName = user.Email?.ToUpper();
                user.PhoneNumber = model.PhoneNumber;
                user.BirthDate = model.BirthDate;
                user.Education = model.Education;
                user.Header = model.Header;
                user.Bio = model.Bio;
                user.Country = model.Country;
                user.ProfilePhoto = model.ProfilePhoto;
                user.CoverPhoto = model.CoverPhoto;
                user.Skills = model.Skills?
                    .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                    .Select(s => new Skill
                    {
                        Name = s.Name?.Trim(),
                        UserId = user.Id
                    }).ToList();

                var updateResult = await _userRepository.UpdateUser(user);

                if (updateResult.IsSuccess)
                    return RedirectToAction("Profile");

                foreach (var error in updateResult.Error)
                    ModelState.AddModelError(string.Empty, error.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Updating user: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the user.");
            }

            return View(model);
        }

    }
}
