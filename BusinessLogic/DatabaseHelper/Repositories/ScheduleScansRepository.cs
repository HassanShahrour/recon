using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Data;
using Reconova.Data.DTOs.Schedule;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class ScheduleScansRepository : IScheduleScansRepository
    {
        private readonly ReconovaDbContext _context;

        public ScheduleScansRepository(ReconovaDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ScheduledScan>>> GetAllScheduledScans(int id)
        {
            try
            {
                var scans = await _context.ScheduledScan.Where(s => s.TaskId == id).Include(s => s.ToolsUsed).ToListAsync();

                if (scans == null || !scans.Any())
                {
                    return Result<List<ScheduledScan>>.Failure("No scans found.");
                }

                return Result<List<ScheduledScan>>.Success(scans);
            }
            catch (Exception ex)
            {
                return Result<List<ScheduledScan>>.Failure($"An error occurred while retrieving scans: {ex.Message}");
            }
        }

        public async Task<Result<ScheduledScan>> GetAllScheduledScanById(int id)
        {
            try
            {
                var scan = await _context.ScheduledScan.FindAsync(id);

                if (scan == null)
                {
                    return Result<ScheduledScan>.Failure("Scan not found.");
                }

                return Result<ScheduledScan>.Success(scan);
            }
            catch (Exception ex)
            {
                return Result<ScheduledScan>.Failure($"An error occurred while retrieving the scan: {ex.Message}");
            }
        }

        public async Task<Result<bool>> AddScheduledScan(ScheduledScan scheduledScan)
        {
            try
            {
                await _context.ScheduledScan.AddAsync(scheduledScan);
                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while adding the scheduled scan: {ex.Message}");
            }
        }

        public async Task<Result<bool>> AddScheduledScanTool(ScheduledTool scheduledTool)
        {
            try
            {
                await _context.ScheduledTool.AddAsync(scheduledTool);
                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while adding the scheduled tool: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateScheduleScan(UpdateScheduleDto dto)
        {
            var scan = await _context.ScheduledScan
                .Include(s => s.ToolsUsed)
                .FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (scan == null)
                return Result<bool>.Failure("Scheduled Scan Not Found");

            scan.Name = dto.Name;

            if (!TimeOnly.TryParse(dto.Time.ToString(), out var parsedTime))
                return Result<bool>.Failure("Invalid time format");

            scan.Time = parsedTime;

            var currentToolNames = scan.ToolsUsed?.Select(t => t.Tool).ToList() ?? new List<string>();

            var toolsToRemove = scan.ToolsUsed?
                .Where(t => !dto.ToolNames.Contains(t.Tool))
                .ToList() ?? new List<ScheduledTool>();

            _context.ScheduledTool.RemoveRange(toolsToRemove);

            var toolsToAdd = dto.ToolNames
                .Except(currentToolNames)
                .Select(name => new ScheduledTool
                {
                    Tool = name,
                    ScheduledScanId = scan.Id
                });

            await _context.ScheduledTool.AddRangeAsync(toolsToAdd);

            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteScheduleScan(int id)
        {
            var scan = await _context.ScheduledScan
                .Include(s => s.ToolsUsed)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (scan == null)
                return Result<bool>.Failure("Scheduled Scan not found");

            _context.ScheduledTool.RemoveRange(scan.ToolsUsed);
            _context.ScheduledScan.Remove(scan);

            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }



    }
}
