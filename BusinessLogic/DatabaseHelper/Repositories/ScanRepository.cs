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

        public async Task<Result<List<ScanResult>>> GetAllScanResults()
        {
            try
            {
                var scanResults = await _context.ScanResults
                    .Where(sr => sr.IsDeleted == 0)
                    //.Include(sr => sr.AIResult)
                    //.Include(sr => sr.User)
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
                    //.Include(sr => sr.AIResult)
                    //.Include(sr => sr.User)
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

        public async Task<Result<bool>> AddScan(string scanId, string target, string command, string output)
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();

                var result = new ScanResult
                {
                    ScanId = scanId,
                    UserId = userId.ToString(),
                    Target = target,
                    Command = command,
                    Output = output,
                };

                _context.ScanResults.Add(result);
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


    }
}
