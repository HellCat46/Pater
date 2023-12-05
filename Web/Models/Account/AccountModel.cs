using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualBasic.CompilerServices;
using Web.Models.Link;

namespace Web.Models.Account;

public class AccountModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    
    [Required]
    [DataType(DataType.EmailAddress)]
    public string email { get; set; }
    
    public string password { get; set; } 
    
    public ICollection<LinkModel> Links { get; set; }
    
    public ExternalAuthModel ExternalAuth { get; set; }
}