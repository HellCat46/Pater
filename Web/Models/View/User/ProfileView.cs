namespace Web.Models.View.User;

public class ProfileView
{
    public string picPath { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    
    
    // Change Password
    public string newpassword { get; set; }
    
    // Delete Account
    public bool Delete { get; set; }
}