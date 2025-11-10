using Reconova.Data.Models;

namespace Reconova.ViewModels.Plans
{
    public class PlansViewModel
    {
        public List<Plan> Plans { get; set; } = new List<Plan>();

        public User User { get; set; } = new User();
    }
}
