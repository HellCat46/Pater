using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Models.Account;

public class ExternalAuthModel
{
    [Key]
    public string UserID { get; set; }
    
    public AuthProvider Provider { get; set; }
    
    public int AccountId { get; set; }
    public AccountModel Account { get; set; }
    
    public enum AuthProvider
    {
        Google,
    }
}