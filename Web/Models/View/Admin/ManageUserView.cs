using Web.Models.Account;
using Web.Models.Link;
using Web.Models.View.User;

namespace Web.Models.View.Admin;

public class ManageUserView
{
    public _HeaderView header { get; set; }
    
    public string userEmail { get;  set; }
    
    // User Information
    public string? UserPicPath { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public DateTime UserAccountCreated { get; set; }
    public AccountModel.Plan UserPlan { get; set; }
    
    // Manage User
    public string UserNewName { get; set; }
    public string UserNewEmail { get; set; }
    public string UserNewPlan { get; set; }
    
    // User Logs 
    public List<ActivityLogModel> logs { get; set; }
    
    // Links
    public List<LinkModel> links { get; set; }
}