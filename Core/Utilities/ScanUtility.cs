using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using System.Diagnostics;


namespace Reconova.Core.Utilities
{
    public class ScanUtility
    {
        private readonly IScanRepository _scanRepository;

        public ScanUtility(IScanRepository scanRepository)
        {
            _scanRepository = scanRepository;
        }

        public async Task<string> StartReconScanAsync(string target, string tool)
        {
            var scanId = Guid.NewGuid().ToString();
            var command = $"{tool} {target}";
            var output = await RunProcessAsync("wsl", $"/snap/bin/{command}");
            await _scanRepository.AddScan(scanId, target, command, output);
            return scanId;
        }

        private async Task<string> RunProcessAsync(string command, string args)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string result = await process.StandardOutput.ReadToEndAsync();
            string errorResult = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(errorResult))
            {
                result += "\nErrors: " + errorResult;
            }

            return result;
        }


    }
}