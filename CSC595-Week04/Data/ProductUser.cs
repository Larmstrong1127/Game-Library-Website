using Microsoft.AspNetCore.Identity;

namespace CSC595_Week04.Data
{
    public class ProductUser : IdentityUser
    {
        public String? FirstName { get; set; }
        public String? LastName { get; set;}
    }
}
