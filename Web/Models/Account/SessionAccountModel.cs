using System.Text.Json;

namespace Web.Models.Account;

public class SessionAccountModel
{
    public int id { get; set; }
    public string name { get; set; }
    public bool isAdmin { get; set; }
    public bool isVerified { get; set; }
    public bool isPaidUser { get; set; }
    public DateTime createdAt { get; set; }
    public string? picPath { get; set; }

    public static void SetSession(HttpContext context, AccountModel acc)
    {
        context.Session.Set("UserData", JsonSerializer.SerializeToUtf8Bytes(new SessionAccountModel()
        {
            id = acc.id,
            createdAt = acc.createdAt,
            isAdmin = acc.isAdmin,
            isVerified = acc.isVerified,
            name = acc.name,
            picPath = acc.picPath,
            isPaidUser = acc.plan != AccountModel.Plan.Free
        }));
    }

    public static SessionAccountModel? GetSession(HttpContext context)
    {
        var bytes = context.Session.Get("UserData");
        if (bytes == null) return null;

        var sessionAccount = JsonSerializer.Deserialize<SessionAccountModel>(bytes);
        if (sessionAccount != null) return sessionAccount;
        
        context.Session.Clear();
        return null;

    }
    public static AccountModel? Deserialize(byte[] bytes)
    {
        return JsonSerializer.Deserialize<AccountModel>(bytes);
    }
}