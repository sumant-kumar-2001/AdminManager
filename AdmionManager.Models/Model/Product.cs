using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminManager.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public double Price { get; set; }   
        public double DiscountAmount { get; set; }
        [Required]
        public int Quantity { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        [ValidateNever]
        public string? ImageUrl { get; set; }
        public string? UserId { get; set; }

    }
}
