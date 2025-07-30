using Reconova.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reconova.ViewModels.Users
{
    public class EditUserViewModel
    {
        [Required]
        public string? Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateOnly BirthDate { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        public string? Header { get; set; }

        public string? Education { get; set; }


        [Display(Name = "Country")]
        public string? Country { get; set; }


        [NotMapped]
        [Display(Name = "Choose a Profile Photo")]
        public IFormFile? ProfilePhoto { get; set; }

        public string? ProfilePhotoPath { get; set; } = "~/images/account-bg.jpg";


        [NotMapped]
        [Display(Name = "Choose a Cover Photo")]
        public IFormFile? CoverPhoto { get; set; }

        public string? CoverPhotoPath { get; set; } = "~/images/account-bg.jpg";

        public List<Skill> Skills { get; set; } = new();
    }
}