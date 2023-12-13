using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
}