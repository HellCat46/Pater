using Web.Models.Account;

namespace Web.Models.View.User;

public class ProfileView
{
    // _Header's Data 
    public string UserName { get; set; }
    public AccountModel.Plans UserPlan { get; set; }
    public string? UserPicPath { get; set; }
    public bool UserIsAdmin { get; set; }
    public string UserEmail { get; set; }
    
    // Change Avatar/Profile Picture
    public IFormFile file { get; set; }
    
    // Change Name and Email
    public string NewName { get; set; }
    public string NewEmail { get; set; }
    
    
    // Change Password
    public string oldPassword { get; set; }
    public string newPassword { get; set; }
    
    
    // Delete Account
    public bool Delete { get; set; }
}