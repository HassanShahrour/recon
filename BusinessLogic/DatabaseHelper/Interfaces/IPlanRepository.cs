using Reconova.BusinessLogic.Exceptions;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface IPlanRepository
    {
        public Task<Result<List<Plan>>> GetAllPlans();
        public Task<Result<Plan>> GetPlanById(int id);
        public Task<Result<bool>> AddPlan(Plan plan);
        public Task<Result<bool>> UpdatePlan(Plan plan);
        public Task<Result<bool>> DeletePlan(int id);
        public Task<Result<bool>> AssignPlanToUserAsync(string userId, int planId);
    }
}
