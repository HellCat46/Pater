using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web.Models.Account;

namespace Web.Models.Link;

public class LinkModel
{
    [Key]
    public string code { get; set; }
    
    [Required]
    public string url { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateOnly CreatedAt { get; set; }
    
    public int AccountId { get; set; }
    public AccountModel Account { get; set; }
}