using Web.Models.Account;

namespace Web.Models.View.User;

public class _HeaderView
{
    public string name { get; set; }
    public bool isPaidUser { get; set; }
    public string? picPath { get; set; }
    public bool isAdmin { get; set; }
}