using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Core.Utilities;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ReconovaDbContext _context;
        private readonly UserUtility _userUtility;

        public TaskRepository(ReconovaDbContext context, UserUtility userUtility)
        {
            _context = context;
            _userUtility = userUtility;
        }

        public async Task<Result<List<Tasks>>> GetAllTasks()
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();

                var tasks = await _context.Tasks
                    .Where(t => t.UserId == userId.ToString() && t.IsDeleted == 0)
                    .OrderByDescending(t => t.LastModified)
                    .ToListAsync();

                if (tasks == null || !tasks.Any())
                {
                    return Result<List<Tasks>>.Failure("No tasks found.");
                }

                return Result<List<Tasks>>.Success(tasks);
            }
            catch (Exception ex)
            {
                return Result<List<Tasks>>.Failure($"An error occurred while retrieving tasks: {ex.Message}");
            }
        }

        public async Task<Result<Tasks>> GetTaskById(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);

                if (task == null || task.IsDeleted == 1)
                {
                    return Result<Tasks>.Failure("Task not found.");
                }

                return Result<Tasks>.Success(task);
            }
            catch (Exception ex)
            {
                return Result<Tasks>.Failure($"An error occurred while retrieving the task: {ex.Message}");
            }
        }

        public async Task<Result<bool>> AddTask(Tasks task)
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();
                task.UserId = userId.ToString();

                await _context.Tasks.AddAsync(task);
                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while adding the task: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateTask(Tasks task)
        {
            try
            {
                var existing = await _context.Tasks.FindAsync(task.Id);
                if (existing == null || existing.IsDeleted == 1)
                    return Result<bool>.Failure("Task not found.");

                existing.Target = task.Target;
                existing.Description = task.Description;
                existing.Percentage = task.Percentage;
                existing.LastModified = task.LastModified;

                _context.Tasks.Update(existing);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while updating the task: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteTask(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null || task.IsDeleted == 1)
                    return Result<bool>.Failure("Task not found.");

                task.IsDeleted = 1;
                _context.Tasks.Update(task);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while deleting the task: {ex.Message}");
            }
        }

    }
}
