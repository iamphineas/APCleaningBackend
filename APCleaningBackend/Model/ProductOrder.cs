namespace APCleaningBackend.Model
{
    public class ProductOrder
    {
        public int ProductOrderID { get; set; }
        public int CustomerID { get; set; }  // Foreign Key to ApplicationUser (Customer)
        public int ProductID { get; set; }  // Foreign Key to Product
        public int Quantity { get; set; }  // Number of products ordered
        public decimal TotalAmount { get; set; }  // Total amount for the order
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;  // Order timestamp

        // Navigation properties
        public virtual ApplicationUser Customer { get; set; }
        public virtual Product Product { get; set; }
    }

}
