using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Enum
{
    public enum UserRole : byte
    {
        [Display(Name = "Role1")]
        Role1 = 1,
        [Display(Name = "Role2")]
        Role2 = 2,
        [Display(Name = "Role3")]
        Role3 = 3
    }
}
