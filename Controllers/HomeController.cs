using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.Data.Models;
using Reconova.ViewModels.Dashboard;
using System.Diagnostics;

namespace Reconova.Controllers
{

    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IToolsRepository _toolsRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IPlanRepository _planRepository;

        public HomeController(ILogger<HomeController> logger,
            IDashboardRepository dashboardRepository,
            IUserRepository userRepository,
            IToolsRepository toolsRepository,
            ICategoryRepository categoryRepository,
            IPlanRepository planRepository)
        {
            _logger = logger;
            _dashboardRepository = dashboardRepository;
            _userRepository = userRepository;
            _toolsRepository = toolsRepository;
            _categoryRepository = categoryRepository;
            _planRepository = planRepository;
        }

        public async Task<IActionResult> Index()
        {
            var topTools = await _dashboardRepository.GetTopUsedToolsAsync();
            var trends = await _dashboardRepository.GetUserRegistrationTrendAsync();
            var categoryData = await _dashboardRepository.GetToolCategoryDistributionAsync();
            var topFollowers = await _dashboardRepository.GetTopFollowedUsersAsync();
            var mostActive = await _dashboardRepository.GetMostActiveUsersAsync();
            var planDistribution = await _dashboardRepository.GetUserPlanDistributionAsync();
            var totalUsers = await _userRepository.GetAllUsers();
            var totalTools = await _toolsRepository.GetAllTools();
            var totalCategories = await _categoryRepository.GetAllCategories();
            var totalPlans = await _planRepository.GetAllPlans();

            var model = new DashboardViewModel
            {
                ToolUsage = topTools ?? new Dictionary<string, int>(),
                RegistrationTrends = trends ?? new Dictionary<string, int>(),
                CategoryDistribution = categoryData ?? new Dictionary<string, int>(),
                TopFollowers = topFollowers ?? new Dictionary<string, int>(),
                ActivityScores = mostActive ?? new Dictionary<string, int>(),
                PlanDistribution = planDistribution ?? new Dictionary<string, int>(),
                TotalUsers = totalUsers?.Value?.Count() ?? 0,
                TotalTools = totalTools?.Value?.Count() ?? 0,
                TotalCategories = totalCategories?.Value?.Count() ?? 0,
                Total = totalPlans.Value?.Count() ?? 0,
            };

            return View(model ?? new DashboardViewModel());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
