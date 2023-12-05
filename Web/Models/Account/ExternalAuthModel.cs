using System.ComponentModel.DataAnnotations;

namespace Web.Models.Account;

public class ExternalAuthModel
{
    [Key]
    public string UserID { get; set; }
    public string Provider { get; set; }
    
    public int AccountId { get; set; }
    public AccountModel Account { get; set; }
}