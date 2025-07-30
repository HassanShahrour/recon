using Reconova.Data.Models;

namespace Reconova.ViewModels.Users
{
    public class ProfileViewModel
    {
        public User? User { get; set; }

        public List<Post> Posts { get; set; } = new List<Post>();
    }
}
