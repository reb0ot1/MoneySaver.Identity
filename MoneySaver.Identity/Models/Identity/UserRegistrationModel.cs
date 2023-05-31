using System.ComponentModel.DataAnnotations;

namespace MoneySaver.Identity.Models.Identity
{
    public class UserRegistrationModel : UserInputModel
    {
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
