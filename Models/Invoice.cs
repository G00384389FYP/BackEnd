using System;
using System.ComponentModel.DataAnnotations;

public class Invoice
{
    [Key]
    public Guid InvoiceId { get; set; }

    [Required]
    public string JobId { get; set; }  

    [Required]
    public int TradesmanId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, ErrorMessage = "Currency code must be 3 characters long.")]
    public string Currency { get; set; } = "EUR";

    [Required]
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    [Required]
    public string? PaymentType { get; set; } 

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    public string? StripePaymentLink { get; set; }

    public string? StripePaymentIntentId { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Notes { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
