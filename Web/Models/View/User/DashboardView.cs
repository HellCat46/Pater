using System.ComponentModel.DataAnnotations;
using Web.Models.Account;
using Web.Models.Link;

namespace Web.Models.View.User;

public class DashboardView
{
    public _HeaderView header { get; set; }
    
    public bool hasLinks { get; set; }
}

