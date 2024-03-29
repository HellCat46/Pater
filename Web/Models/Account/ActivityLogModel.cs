using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web.ApplicationDbContext;


namespace Web.Models.Account;

public class ActivityLogModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    
    public DateTime date { get; set; }
    
    public int Userid { get; set; }
    public AccountModel User { get; set; }
    
    public string IPAddr { get; set; }
    
    public string Action { get; set; }

    
    public static void WriteLogs(UserDbContext context, Event ev, string ip, int accId, string accName, string accEmail = "Add Email to Function Call" )
    {
        ActivityLogModel log = new ActivityLogModel()
        {
            date = DateTime.Now,
            IPAddr = ip,
            Userid = accId,
            Action = accName
        };

        switch (ev)
        {
            case Event.EmailSignedUp: log.Action +=  " Signed up with Email.";
                break;
            case Event.EmailLoggedIn: log.Action +=  " Logged in with Email.";
                break;
            case Event.GoogleSignedUp: log.Action += " Signed up with Google.";
                break;
            case Event.GoogleLoggedIn: log.Action += " Logged in with Google.";
                break;
            case Event.LoggedOut: log.Action +=  " Logged Out.";
                break;
            case Event.VerifyEmail: log.Action += " Verified their Email.";
                break;
            case Event.ChangedAvatar: log.Action +=  " Changed their Profile Picture.";
                break;
            case Event.ResetPassword : log.Action += " Resets their Password.";
                break;
            case Event.ChangedPassword: log.Action +=  " Changed their Password.";
                break;
            case Event.ChangedEmail: log.Action +=  " Changed their Email to " + accEmail + ".";
                break;
            case Event.ChangedName: log.Action +=  " Changed their Name to "+ accName + ".";
                break;
            case Event.CreatedLink: log.Action +=  " Created a new link.";
                break;
            case Event.EditLink: log.Action +=  " Edited Link Details.";
                break;
            case Event.DeleteLink: log.Action += " Deleted a link.";
                break;
            default: return;
        }

        try
        {
            context.ActivityLogs.Add(log);
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.Write(ex);
        }
    }
    public enum Event
    {
        EmailLoggedIn,
        GoogleLoggedIn,
        EmailSignedUp,
        GoogleSignedUp,
        LoggedOut,
        VerifyEmail,
        ChangedAvatar,
        ResetPassword,
        ChangedPassword,
        ChangedEmail,
        ChangedName, 
        CreatedLink,
        EditLink,
        DeleteLink,
    }
    
}