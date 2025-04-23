using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateProductDto
{   
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]    // required doesn't work in number types
    public decimal Price { get; set; }

    [Required]
    public string PictureUrl { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Brand { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Quantity in stock must be at least 1")]    // required doesn't work in number types 
    public int QuantityInStock { get; set; }
}
