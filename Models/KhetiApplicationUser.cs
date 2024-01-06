﻿using Microsoft.AspNetCore.Identity;

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


    }
}
