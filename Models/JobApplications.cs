using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class JobApplications
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid JobId { get; set; }

    [Required]
    public Guid TradesmanId { get; set; }

    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "pending"; // 'pending', 'accepted', 'declined', 'withdrawn'

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    
}
