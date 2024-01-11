using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Web;

public class MailingSystem
{
    public static void SendPasswordReset(MailServer mailServer, string receiverName,string receiverMail, string link)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Pater", "no-reply@pater.hellcat46.works"));
        message.To.Add(new MailboxAddress(receiverName, receiverMail));
        message.Subject = "Password Reset Request";
        message.Body = new TextPart(TextFormat.Text) { Text = "Go to "+link}; // Great Design ik ik
        
        using (var client = new SmtpClient())
        {
            client.Connect(mailServer.SMTPServer, mailServer.ServerPort, SecureSocketOptions.SslOnConnect);
            
            client.Authenticate(mailServer.Email, mailServer.Password);

            client.Send(message);
            
            client.Disconnect(true);
        }
        Console.Write("SEND EMAIL");
    }
}

public record MailServer {
    public string SMTPServer { get; set; }
    public int ServerPort { get; set; }
    public string Email { get; set; }
    public string Password {get; set; }

}