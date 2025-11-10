using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Core.Utilities;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class ScanRepository : IScanRepository
    {

        private readonly ReconovaDbContext _context;
        private readonly UserUtility _userUtility;

        public ScanRepository(ReconovaDbContext context, UserUtility userUtility)
        {
            _context = context;
            _userUtility = userUtility;
        }

        public async Task<Result<List<ScanResult>>> GetAllScanResults(int id)
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();

                var scanResults = await _context.ScanResults
                    .Where(sr => sr.UserId == userId.ToString() && sr.TaskId == id && sr.IsDeleted == 0)
                    .Include(sr => sr.AIResult)
                    .Include(sr => sr.User)
                    .OrderByDescending(sr => sr.Timestamp)
                    .ToListAsync();

                if (scanResults == null || !scanResults.Any())
                {
                    return Result<List<ScanResult>>.Failure("No scan results found.");
                }

                return Result<List<ScanResult>>.Success(scanResults);
            }
            catch (InvalidDataException ex)
            {
                return Result<List<ScanResult>>.Failure($"Invalid data: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching scan results: " + ex.Message);
                return Result<List<ScanResult>>.Failure("An error occurred while fetching scan results.");
            }
        }

        public async Task<Result<ScanResult>> GetScanResultById(string id)
        {
            try
            {
                var scan = await _context.ScanResults
                    .Include(sr => sr.AIResult)
                    .Include(sr => sr.User)
                    .FirstOrDefaultAsync(sr => sr.ScanId == id && sr.IsDeleted == 0);

                if (scan == null)
                {
                    return Result<ScanResult>.Failure($"No scan result found");
                }

                return Result<ScanResult>.Success(scan);
            }
            catch (InvalidDataException ex)
            {
                return Result<ScanResult>.Failure($"Invalid data: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching scan: " + ex.Message);
                return Result<ScanResult>.Failure("An error occurred while fetching scan result.");
            }
        }

        public async Task<Result<bool>> AddScan(string userId, string scanId, int taskId, string target, string tool, string command, string output, string reply)
        {
            try
            {
                var result = new ScanResult
                {
                    ScanId = scanId,
                    UserId = userId.ToString(),
                    TaskId = taskId,
                    Target = target,
                    Tool = tool,
                    Command = command,
                    Output = output,
                };

                var analysis = new AIResult
                {
                    ScanId = scanId,
                    UserId = userId.ToString(),
                    TaskId = taskId,
                    Output = reply,
                };

                _context.ScanResults.Add(result);
                _context.AIResults.Add(analysis);

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (InvalidDataException ex)
            {
                return Result<bool>.Failure($"Invalid data: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving scan result: " + ex.Message);
                return Result<bool>.Failure("An error occurred while saving the scan result.");
            }
        }

        public async Task<Result<bool>> DeleteScan(int id)
        {
            try
            {
                var scan = await _context.ScanResults.FindAsync(id);

                if (scan == null || scan.IsDeleted == 1)
                {
                    return Result<bool>.Failure("Scan not found or already deleted.");
                }

                scan.IsDeleted = 1;
                _context.ScanResults.Update(scan);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (InvalidDataException ex)
            {
                return Result<bool>.Failure($"Invalid data: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting scan: " + ex.Message);
                return Result<bool>.Failure("An error occurred while deleting the scan.");
            }
        }


        public async Task<Result<bool>> CanUserScanToday()
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();

                var user = await _context.Users
                    .Include(u => u.Plan)
                    .FirstOrDefaultAsync(u => u.Id == userId.ToString() && u.IsDeleted == 0);

                if (user == null || user.Plan == null || !user.IsPlanActive)
                    return Result<bool>.Failure($"Invalid data");

                if (user.Plan.MaxScansPerDay == 0)
                    Result<bool>.Failure($"Invalid data");

                var today = DateTime.UtcNow.Date;

                int scansToday = await _context.ScanResults
                    .CountAsync(s =>
                        s.UserId == userId.ToString() &&
                        s.Timestamp.Date == today &&
                        s.IsDeleted == 0);

                return Result<bool>.Success(scansToday < user.Plan.MaxScansPerDay);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking scan limit: " + ex.Message);
                return Result<bool>.Failure("An error occurred while checking the scan limit.");
            }
        }


    }
}
