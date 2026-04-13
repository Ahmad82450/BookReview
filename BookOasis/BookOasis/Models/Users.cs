using Microsoft.AspNetCore.Identity;

namespace BookOasis.Models
{
    public class Users : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
