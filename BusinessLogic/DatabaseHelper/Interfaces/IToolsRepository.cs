using Reconova.BusinessLogic.Exceptions;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface IToolsRepository
    {
        Task<Result<List<Tool>>> GetAllTools();

        Task<Result<Tool>> GetToolById(int id);

        Task<Result<bool>> AddTool(Tool tool);

        Task<Result<bool>> UpdateTool(Tool tool);

        Task<Result<bool>> DeleteTool(int id);
    }
}
