using Web.Models.Account;
using Web.Models.View.User;

namespace Web.Models.View.Admin;

public class AdminDashboardView
{
    public _HeaderView Header { get; set; }
    public string userEmail { get; set; }
}