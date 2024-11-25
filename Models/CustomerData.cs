using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CustomerData
{
    [Key]
    [ForeignKey("UserData")]
    public int UserId { get; set; }

    public int JobsPosted { get; set; } = 0;

    public bool IsSuspended { get; set; } = false;

    public DateTime DateAdded { get; set; } = DateTime.Now;

    // navigation property to allow lazy loading of related UserData
    public UserData UserData { get; set; }
}