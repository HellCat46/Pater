using System.ComponentModel.DataAnnotations;

namespace Web.Models.Account;

public class SessionModel
{
    [Key]
    public string id { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime expiryDate { get; set; }
    
    public int AccountId { get; set; }
    public AccountModel Account { get; set; }
}