
namespace AdminManager.Models
{
    public class Discount
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime ValidFrom { get; set; } 
        public DateTime ValidTo { get; set; } 
        public double DiscountAmount{ get; set; }
        public string DiscountType { get; set; }    
         
    }
}
