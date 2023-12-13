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
    
    
    public bool Changes { get; set; }
    public IFormFile File { get; set; }
    public string NewName { get; set; }
    public string NewEmail { get; set; }
    public string OldPassword { get; set; }
    // Change Password
    public string NewPassword { get; set; }
    
    // Delete Account
    public bool Delete { get; set; }
}