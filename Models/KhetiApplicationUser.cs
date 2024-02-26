using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kheti.Models
{
    public class KhetiApplicationUser  : IdentityUser
    {
        public string FirstName { get; set; }   
        public string LastName { get; set; }   

        public string District { get; set; }

        public string Address { get; set; }
        public DateTime RegistrationDate { get; set; }

        public string ProfilePictureURL { get; set; }
        
        // Navigation property for comments
        public ICollection<ProductComment> ProductComments { get; set; }

        // Navigation property for replies
        public ICollection<ProductReply> Replies { get; set; } 

        //Navigation Property to link to ExpertProfile
        public ExpertProfile ExpertProfile { get; set; }

        //added later
        public string province { get; set; }
        public string LocalAddress { get; set; }
        public string AdditionalPhoneNumber { get; set; }
    }
}
