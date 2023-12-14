using System.ComponentModel.DataAnnotations;

namespace Web.Models.View;

public class SignupView
{
    public string name { get; set; }
    
    [EmailAddress]
    public string email { get; set; }
    
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,50}$", ErrorMessage = "Password doesn't meet the requirements. (Minimum 8 Character, 1 Number, 1 Lowercase and 1 Uppercase)")]
    public string password { get; set; }
}