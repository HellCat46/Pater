using System.ComponentModel.DataAnnotations;

namespace Web.Models.View;

public class SignupView
{
    public string name { get; set; }
    
    [EmailAddress]
    public string email { get; set; }
    
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$", ErrorMessage = "Your Password Sucks")]
    public string password { get; set; }
}