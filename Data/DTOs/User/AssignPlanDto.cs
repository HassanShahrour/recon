using System.ComponentModel.DataAnnotations;

namespace Reconova.Data.DTOs.User
{
    public class AssignPlanDto
    {
        [Required]
        public string? UserId { get; set; }

        [Required]
        public int PlanId { get; set; }
    }
}
