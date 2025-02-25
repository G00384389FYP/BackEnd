using System;
using System.ComponentModel.DataAnnotations;
    
public class JobApplications
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid JobId { get; set; }

    [Required]
    public int TradesmanId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "pending"; // 'pending', 'accepted', 'declined', 'withdrawn'

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
