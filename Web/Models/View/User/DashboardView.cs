using Web.Models.Account;
using Web.Models.Link;

namespace Web.Models.View.User;

public class DashboardView
{
    public string name { get; set; }
    
    public AccountModel.Plans plan { get; set; }
    public string? picPath { get; set; }
    public bool isAdmin { get; set; }
    
    public List<LinkModel> links { get; set; }
}

