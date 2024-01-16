using System.ComponentModel.DataAnnotations;

namespace Web.Models.View.Home;

public class SignupView
{
    public string name { get; set; }
    public string email { get; set; }
    public string password { get; set; }
}