using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.Inputs
{
    public class RegisterInput
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public Enum.UserRole Role { get; set; }

    }
}
