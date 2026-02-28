using System.ComponentModel.DataAnnotations;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, ErrorMessage = "Name must be <= 50 chars")]
    public string Name { get; set; }

    [Range(1, 100000, ErrorMessage = "Price must be between 1 and 100000")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [StringLength(30, ErrorMessage = "Category must be <= 30 chars")]
    public string Category { get; set; }
}