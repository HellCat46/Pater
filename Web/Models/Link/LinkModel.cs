using System.ComponentModel.DataAnnotations;
using Web.Models.Account;

namespace Web.Models.Link;

public class LinkModel
{
    [Key]
    public string code { get; set; }
    
    [Required]
    public string url { get; set; }
    
    [Required]
    public string name { get; set; }
    
    [Required]
    public DateTime LastModified { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public bool isPaid { get; set; }
    
    public int AccountId { get; set; }
    public AccountModel Account { get; set; }
}