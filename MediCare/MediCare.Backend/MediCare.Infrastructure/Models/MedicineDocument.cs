namespace MediCare.Infrastructure.Models
{
    /// <summary>
    /// Elasticsearch document za Full-Text Search
    /// </summary>
    public class MedicineDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string ImagePath { get; set; }
        public int Weight { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
