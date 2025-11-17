using Microsoft.AspNetCore.Identity;
using WebAPI_ESOChallenge.Features.Purchases.Models;

namespace WebAPI_ESOChallenge.Features.Authentication.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Additional properties can be added here
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        /// <summary>
        /// V-Bucks balance. New users receive 10,000 V-Bucks.
        /// </summary>
        public int VBucks { get; set; } = 10000;
        
        // Navigation property for purchased cosmetics
        public virtual ICollection<UserCosmetic> PurchasedCosmetics { get; set; } = new List<UserCosmetic>();
    }
}