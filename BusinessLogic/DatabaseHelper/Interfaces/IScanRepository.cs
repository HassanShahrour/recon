using Reconova.BusinessLogic.Exceptions;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface IScanRepository
    {

        Task<Result<List<ScanResult>>> GetAllScanResults();

        Task<Result<ScanResult>> GetScanResultById(string id);

        Task<Result<bool>> AddScan(string scanId, string target, string command, string output);
    }
}
