namespace APCleaningBackend.Model
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }  // Name of the product
        public string Description { get; set; }  // Detailed description
        public decimal Price { get; set; }  // Price of the product
        public int StockQuantity { get; set; }  // Quantity available in stock
        public string Category { get; set; }  // Category (e.g., "Cleaning Supplies")
        public bool IsAvailable { get; set; } = true;  // If the product is available for sale

        public string ProductImageUrl { get; set; }  // URL to the product image

    }

}
