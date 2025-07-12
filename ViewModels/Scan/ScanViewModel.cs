using Reconova.Data.Models;

namespace Reconova.ViewModels.Scan
{
    public class ScanViewModel
    {
        public List<ScanResult> ScanResults { get; set; } = new List<ScanResult>();

        public List<Category> Categories { get; set; } = new List<Category>();

        public List<Tool> Tools { get; set; } = new List<Tool>();
    }
}
