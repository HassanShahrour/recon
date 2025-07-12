using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class ToolsRepository : IToolsRepository
    {
        private readonly ReconovaDbContext _context;

        public ToolsRepository(ReconovaDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<Tool>>> GetAllTools()
        {
            try
            {
                var tools = await _context.Tools.Where(t => t.IsDeleted == 0).ToListAsync();

                if (tools == null || !tools.Any())
                {
                    return Result<List<Tool>>.Failure("No tools found.");
                }

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
    }
}
