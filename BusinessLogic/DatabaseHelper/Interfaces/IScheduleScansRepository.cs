using Reconova.BusinessLogic.Exceptions;
using Reconova.Data.DTOs.Schedule;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface IScheduleScansRepository
    {
        public Task<Result<List<ScheduledScan>>> GetAllScheduledScans(int id);

        public Task<Result<ScheduledScan>> GetAllScheduledScanById(int id);

        public Task<Result<bool>> AddScheduledScan(ScheduledScan scheduledScan);

        public Task<Result<bool>> AddScheduledScanTool(ScheduledTool scheduledTool);

        public Task<Result<bool>> UpdateScheduleScan(UpdateScheduleDto dto);

        public Task<Result<bool>> DeleteScheduleScan(int id);
    }
}
