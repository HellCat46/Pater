using Web.Models.Account;

namespace Web.Models.View.Admin;

public class UpdateUser
{
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public AccountModel.Plan? Plan { get; set; }
    public string? Password { get; set; }
    public int? LinkLimit { get; set; }
    public bool? Verified { get; set; }
}