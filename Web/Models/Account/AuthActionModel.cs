using System.ComponentModel.DataAnnotations;

namespace Web.Models.Account;

public class AuthActionModel
{
    [Key] 
    [StringLength(50)] 
    public string code { get; set; }
    
    public ActionType action { get; set; }
    
    public int Userid { get; set; }
    public AccountModel User { get; set; }
    
    public DateTime createAt { get; set; }

    public enum ActionType
    {
        VerifyEmail,
        ResetPassword
    }
}