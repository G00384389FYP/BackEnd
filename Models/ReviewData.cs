using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ReviewData
{
    [Key]
    public Guid ReviewId { get; set; } 
    
    [Required]
    public Guid JobId { get; set; }  

    [Required]
    [ForeignKey("Invoice")]
    public Guid InvoiceId { get; set; } = Guid.Empty;

    [Required]
    [ForeignKey("UserData")]
    public int ReviewerId { get; set; }

    [Range(1, 5, ErrorMessage = "WorkRating must be between 1 and 5.")]
    public int WorkRating { get; set; } = 0;

    [Range(1, 5, ErrorMessage = "CustomerServiceRating must be between 1 and 5.")]
    public int CustomerServiceRating { get; set; } = 0;

    [Range(1, 5, ErrorMessage = "PriceRating must be between 1 and 5.")]
    public int PriceRating { get; set; } = 0;

    public string? Comment { get; set; } = null;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}