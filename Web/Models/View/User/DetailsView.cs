using Web.Models.Account;
using Web.Models.Link;

namespace Web.Models.View.User;

public class DetailsView
{
    public _HeaderView header { get; set; }
    
    public LinkModel linkDetails { get; set; }
    
    public AccountModel.Plan userPlan { get; set; }
}