using Microsoft.AspNetCore.Identity;

namespace WebAPI_ESOChallenge.Features.Authentication.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Additional properties can be added here
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}