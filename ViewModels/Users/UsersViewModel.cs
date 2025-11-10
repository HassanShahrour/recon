using Reconova.Data.Models;

namespace Reconova.ViewModels.Users
{
    public class UsersViewModel
    {
        public List<User> Users { get; set; } = new List<User>();

        public List<Plan> Plans { get; set; } = new List<Plan>();
    }
}
