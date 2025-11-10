using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Core.Utilities;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class ToolsRepository : IToolsRepository
    {
        private readonly ReconovaDbContext _context;
        private readonly UserUtility _userUtility;
        private readonly IUserRepository _userRepository;

        public ToolsRepository(ReconovaDbContext context, UserUtility userUtility, IUserRepository userRepository)
        {
            _context = context;
            _userUtility = userUtility;
            _userRepository = userRepository;
        }

        public async Task<Result<List<Tool>>> GetAllTools()
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();
                var userResult = await _userRepository.GetUserById(userId.ToString());

                if (!userResult.IsSuccess || userResult.Value == null)
                    return Result<List<Tool>>.Failure("User not found.");

                var user = userResult.Value;
                var tools = new List<Tool>();

                if (user.Role == "User")
                {
                    var userPlan = await _context.Plan.FirstOrDefaultAsync(p => p.Id == user.PlanId);
                    if (userPlan == null)
                        return Result<List<Tool>>.Failure("User's plan not found.");

                    var accessiblePlanIds = await _context.Plan
                        .Where(p => p.Priority <= userPlan.Priority)
                        .Select(p => p.Id)
                        .ToListAsync();

                    tools = await _context.Tools
                        .Where(t => accessiblePlanIds.Contains(t.PlanId) && t.IsDeleted == 0)
                        .Include(t => t.Category)
                        .Include(t => t.Plan)
                        .OrderBy(t => t.Name)
                        .ToListAsync();
                }
                else
                {
                    tools = await _context.Tools
                        .Where(t => t.IsDeleted == 0)
                        .Include(t => t.Category)
                        .Include(t => t.Plan)
                        .OrderBy(t => t.Name)
                        .ToListAsync();
                }

                if (tools == null || !tools.Any())
                    return Result<List<Tool>>.Failure("No tools found.");

                return Result<List<Tool>>.Success(tools);
            }
            catch (Exception ex)
            {
                return Result<List<Tool>>.Failure($"An error occurred while retrieving tools: {ex.Message}");
            }
        }


        public async Task<Result<Tool>> GetToolById(int id)
        {
            try
            {
                var tool = await _context.Tools.FindAsync(id);

                if (tool == null || tool.IsDeleted == 1)
                {
                    return Result<Tool>.Failure("Tool not found.");
                }

                return Result<Tool>.Success(tool);
            }
            catch (Exception ex)
            {
                return Result<Tool>.Failure($"An error occurred while retrieving the tool: {ex.Message}");
            }
        }

        public async Task<Result<bool>> AddTool(Tool tool)
        {
            try
            {
                await _context.Tools.AddAsync(tool);
                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while adding the tool: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateTool(Tool tool)
        {
            try
            {
                var existing = await _context.Tools.FindAsync(tool.Id);
                if (existing == null || tool.IsDeleted == 1)
                    return Result<bool>.Failure("Tool not found.");

                existing.Name = tool.Name;
                existing.CategoryId = tool.CategoryId;
                existing.Description = tool.Description;
                existing.PlanId = tool.PlanId;

                _context.Tools.Update(existing);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while updating the tool: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteTool(int id)
        {
            try
            {
                var tool = await _context.Tools.FindAsync(id);
                if (tool == null || tool.IsDeleted == 1)
                    return Result<bool>.Failure("Tool not found.");


                tool.IsDeleted = 1;
                _context.Tools.Update(tool);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while deleting the tool: {ex.Message}");
            }
        }

        public async Task<int?> GetToolIdByName(string name)
        {
            return await _context.Tools
                .Where(t => t.Name == name)
                .Select(t => (int?)t.Id)
                .FirstOrDefaultAsync();
        }


    }
}
