using Reconova.BusinessLogic.Exceptions;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface IScanRepository
    {

        Task<Result<List<ScanResult>>> GetAllScanResults(int id);

        Task<Result<ScanResult>> GetScanResultById(string id);

        Task<Result<bool>> AddScan(string userId, string scanId, int taskId, string target, string tool, string command, string output, string reply);

        Task<Result<bool>> DeleteScan(int id);

        Task<Result<bool>> CanUserScanToday();
    }
}
