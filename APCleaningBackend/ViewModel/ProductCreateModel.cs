namespace APCleaningBackend.ViewModel
{
    public class ProductCreateModel
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; }
        public bool IsAvailable { get; set; }
        public IFormFile? ProductImage { get; set; } // Optional image
    }

}
