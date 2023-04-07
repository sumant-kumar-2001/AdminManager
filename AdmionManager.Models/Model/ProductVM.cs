using AdminManager.Models;


namespace AdminManager.Models
{
    public  class ProductVM 
    {
        public Product Name { get; set; }
        public Product Description { get; set; }
        public Product IsActive { get; set; }
        public Product Quantity { get; set; }
        public Product Price { get; set; }
    }
}
