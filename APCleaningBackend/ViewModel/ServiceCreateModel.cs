namespace APCleaningBackend.ViewModel
{
    public class ServiceCreateModel
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }
        public IFormFile ServiceImage { get; set; }
    }
}
