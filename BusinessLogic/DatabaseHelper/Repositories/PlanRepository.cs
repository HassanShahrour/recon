using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class PlanRepository : IPlanRepository
    {
        private readonly ReconovaDbContext _context;

        public PlanRepository(ReconovaDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<Plan>>> GetAllPlans()
        {
            try
            {
                var plans = await _context.Plan
                    .Where(p => p.IsDeleted == 0)
                    .Include(p => p.Tools)
                    .OrderBy(p => p.Price)
                    .ToListAsync();

                if (plans == null || !plans.Any())
                {
                    return Result<List<Plan>>.Failure("No plans found.");
                }

                return Result<List<Plan>>.Success(plans);
            }
            catch (Exception ex)
            {
                return Result<List<Plan>>.Failure($"An error occurred while retrieving plans: {ex.Message}");
            }
        }

        public async Task<Result<Plan>> GetPlanById(int id)
        {
            try
            {
                var plan = await _context.Plan.FindAsync(id);

                if (plan == null || plan.IsDeleted == 1)
                {
                    return Result<Plan>.Failure("Plan not found.");
                }

                return Result<Plan>.Success(plan);
            }
            catch (Exception ex)
            {
                return Result<Plan>.Failure($"An error occurred while retrieving the plan: {ex.Message}");
            }
        }

        public async Task<Result<bool>> AddPlan(Plan plan)
        {
            try
            {
                await _context.Plan.AddAsync(plan);
                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while adding the plan: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdatePlan(Plan plan)
        {
            try
            {
                var existing = await _context.Plan.FindAsync(plan.Id);
                if (existing == null || existing.IsDeleted == 1)
                    return Result<bool>.Failure("Plan not found.");

                existing.Name = plan.Name;
                existing.Description = plan.Description;
                existing.Price = plan.Price;
                existing.DurationInDays = plan.DurationInDays;
                existing.MaxScansPerDay = plan.MaxScansPerDay;
                existing.CanGenerateReport = plan.CanGenerateReport;
                existing.Priority = plan.Priority;

                _context.Plan.Update(existing);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while updating the plan: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeletePlan(int id)
        {
            try
            {
                var plan = await _context.Plan.FindAsync(id);
                if (plan == null || plan.IsDeleted == 1)
                    return Result<bool>.Failure("Plan not found.");

                plan.IsDeleted = 1;
                _context.Plan.Update(plan);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while deleting the plan: {ex.Message}");
            }
        }

        public async Task<Result<bool>> AssignPlanToUserAsync(string userId, int planId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return Result<bool>.Failure("User not found.");

                var plan = await _context.Plan.FindAsync(planId);
                if (plan == null)
                    return Result<bool>.Failure("Plan not found.");

                user.PlanId = plan.Id;
                user.PlanStartDate = DateTime.UtcNow;
                user.PlanEndDate = DateTime.UtcNow.AddDays(plan.DurationInDays);
                user.IsPlanActive = true;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error assigning plan to user: " + ex.Message);
                return Result<bool>.Failure("An error occurred while assigning the plan: " + ex.Message);
            }
        }



    }
}
