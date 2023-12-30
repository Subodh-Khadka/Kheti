using System.ComponentModel.DataAnnotations;

namespace Kheti.Models
{
    public class Category
    {
        [Key]
        public Guid CategoryId { get; set; }
        [Required]
        public string CategoryName { get; set; }

    }
}
