using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Web;

public class MailingSystem
{
    public static void SendWelcomeMail(MailServer mailServer, string receiverName, string receiverMail)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Pater", "no-reply@pater.hellcat46.works"));
        message.To.Add(new MailboxAddress(receiverName, receiverMail));
        message.Subject = "Welcome to Pater";
        message.Body = new TextPart(TextFormat.Text) { Text = string.Format("Hi {0}, \n Thanks for Choosing Pater and... Bye.", receiverName)};
        
        SendMail(mailServer, message);
    }
    public static void SendPasswordReset(MailServer mailServer, string receiverName,string receiverMail, string link)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Pater", "no-reply@pater.hellcat46.works"));
        message.To.Add(new MailboxAddress(receiverName, receiverMail));
        message.Subject = "Password Reset Request";
        message.Body = new TextPart(TextFormat.Text) { Text = "Reset your Email at "+link}; // Great Design ik ik
        
        SendMail(mailServer, message);
    }
    
    public static void SendEmailVerification(MailServer mailServer, string receiverName,string receiverMail, string link)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Pater", "no-reply@pater.hellcat46.works"));
        message.To.Add(new MailboxAddress(receiverName, receiverMail));
        message.Subject = "Email Verification";
        message.Body = new TextPart(TextFormat.Text) { Text = "Verify your email at "+link}; // Great Design ik ik
        
        SendMail(mailServer, message);
    }

    private static void SendMail(MailServer mailServer, MimeMessage message)
    {
        using (var client = new SmtpClient())
        {
            client.Connect(mailServer.SMTPServer, mailServer.ServerPort, SecureSocketOptions.SslOnConnect);
            
            client.Authenticate(mailServer.Email, mailServer.Password);

            client.Send(message);
            
            client.Disconnect(true);
        }
    }
}

public record MailServer {
    public string SMTPServer { get; set; }
    public int ServerPort { get; set; }
    public string Email { get; set; }
    public string Password {get; set; }

}