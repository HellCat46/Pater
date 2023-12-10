using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web.Models.Link;

namespace Web.Models.Account;

public class AccountModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(Order = 0)]
    public int id { get; set; }
    
    [Required]
    [DataType(DataType.EmailAddress)]
    [Column(Order = 1)]
    public string email { get; set; }
    
    [Required]
    [Column(Order = 5)]
    public bool isAdmin { get; set; }

    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime createdAt { get; set; }
    
    [Column(Order = 3)]
    public string name { get; set; }
    
    [Column(Order = 2)]
    public string password { get; set; } 
    
    [Column(Order = 4)]
    public string picPath { get; set; }
    
    public ExternalAuthModel ExternalAuth { get; set; }
    
    public ICollection<LinkModel> Links { get; set; }
    
    public ICollection<ActivityLogsModel> Logs { get; set; }
}