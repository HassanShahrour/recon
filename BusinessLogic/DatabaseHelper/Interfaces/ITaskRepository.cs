using Reconova.BusinessLogic.Exceptions;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface ITaskRepository
    {
        public Task<Result<List<Tasks>>> GetAllTasks();

        public Task<Result<Tasks>> GetTaskById(int id);

        Task<Result<bool>> AddTask(Tasks task);

        Task<Result<bool>> UpdateTask(Tasks task);

        Task<Result<bool>> DeleteTask(int id);
    }
}
