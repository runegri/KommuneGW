using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Models
{

    public class ApplicationUser : IdentityUser
    {
        public bool IsBlocked { get; internal set; }
        public bool IsDeleted { get; internal set; }
    }
}
