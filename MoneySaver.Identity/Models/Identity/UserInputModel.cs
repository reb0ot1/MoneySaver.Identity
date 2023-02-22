using System.ComponentModel.DataAnnotations;
using static MoneySaver.Identity.Data.DataConstants.Identity;

namespace MoneySaver.Identity.Models.Identity
{
    public class UserInputModel
    {
        [EmailAddress]
        [Required]
        [MinLength(MinEmailLength)]
        [MaxLength(MaxEmailLength)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}   
