using System.Text.Json;
using Web.Models.Account;

namespace Web.Models;

[Serializable]
public class UserData
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    
    public bool UserisAdmin { get; set; }
    public bool UserisVerified { get; set; }
    public AccountModel.Plans UserPlan { get; set; }
    public string? UserPassword { get; set; } 
    
    public string? PicPath { get; set; }

    public static byte[] Serialize(UserData userData)
    {
        return JsonSerializer.SerializeToUtf8Bytes(userData);
    }
    public static UserData Deserialize(byte[] bytes)
    {
        return JsonSerializer.Deserialize<UserData>(bytes);
    }

    
}