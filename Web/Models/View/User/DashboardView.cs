using System.ComponentModel.DataAnnotations;
using Web.Models.Account;
using Web.Models.Link;

namespace Web.Models.View.User;

public class DashboardView
{
    // _Header's Data 
    public string UserName { get; set; }
    public AccountModel.Plans UserPlan { get; set; }
    public string? UserPicPath { get; set; }
    public bool UserIsAdmin { get; set; }
    
    
    // Body's Data
    public List<LinkModel> links { get; set; }
    
    // Create Link
    public string NewLinkURL { get; set; }
    public string NewLinkName { get; set; }
    public string NewLinkCode { get; set; }
}

