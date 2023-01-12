using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.Inputs
{
    public class ForgetPasswordInput
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Url]
        public string CallbackUrl { get; set; }
    }
}
