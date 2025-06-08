namespace Backend.Shared.DTOs{
    public class ProductDto {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<AnimalCategoryDto> AnimalCategories { get; set; } = new List<AnimalCategoryDto>();
    }
        public class AnimalCategoryDto {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}