using Web.Models.Account;

namespace Web.Models.View.User;

public class ProfileView
{
    
    public _HeaderView header { get; set; }
    
    public string UserName { get; set; }
    public string UserEmail { get; set; }
}