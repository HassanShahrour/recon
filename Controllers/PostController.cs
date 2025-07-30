using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Core.Utilities;
using Reconova.Data.Models;

namespace Reconova.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly UserUtility _userUtility;

        public PostController(IPostRepository postRepository, UserUtility userUtility)
        {
            _postRepository = postRepository;
            _userUtility = userUtility;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();
                ViewBag.UserId = userId.ToString();

                var result = await _postRepository.GetAllPosts();
                if (result.IsSuccess && result.Value != null)
                {
                    return View(result.Value);
                }

                TempData["Error"] = result.Error ?? "Failed to load posts.";
                return View(new List<Post>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return View(new List<Post>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [RequestSizeLimit(524288000)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post)
        {
            if (!ModelState.IsValid)
                return View(post);

            try
            {
                var result = await _postRepository.CreatePostAsync(post, post.UploadedFiles);

                if (result.IsSuccess)
                {
                    TempData["Success"] = "Post created successfully.";
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create post.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the post.");
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return View(post);
        }
    }
}
