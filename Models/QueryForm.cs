using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System;



namespace Kheti.Models
{
    public class QueryForm
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email {  get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Location { get; set; }
        public string ProblemCategory { get; set; }
        [ValidateNever]
        public string UrgencyLevel { get; set; }
        [Required]
        [ValidateNever]
        public String ImageUrl{ get; set; }
        [Required]
        public DateTime? DateCreated { get; set; }

        [ValidateNever]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public KhetiApplicationUser User { get; set; }

    }
}
