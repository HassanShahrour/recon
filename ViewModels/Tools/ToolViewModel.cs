using Reconova.Data.Models;

namespace Reconova.ViewModels.Tools
{
    public class ToolViewModel
    {

        public List<Tool> Tools { get; set; } = new List<Tool>();

        public List<Category> Categories { get; set; } = new List<Category>();

        public List<Plan> Plans { get; set; } = new List<Plan>();
    }
}
