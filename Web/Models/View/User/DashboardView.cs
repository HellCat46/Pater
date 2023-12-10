using Web.Models.Link;

namespace Web.Models.View.User;

public class DashboardView
{
    public string picPath { get; set; }
    public bool isAdmin { get; set; }
    
    public List<LinkModel> links { get; set; }
}