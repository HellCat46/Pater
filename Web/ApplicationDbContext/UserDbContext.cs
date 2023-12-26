using Microsoft.EntityFrameworkCore;
using Web.Models;
using Web.Models.Account;
using Web.Models.Link;

namespace Web.ApplicationDbContext;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions options) : base(options) 
    {
        
    }
    
    public DbSet<LinkModel> Link { get; set; }
    public DbSet<AccountModel> Account { get; set; }
    public DbSet<ExternalAuthModel> ExternalAuth { get; set; }
    public DbSet<ActivityLogModel> ActivityLogs { get; set; }
    public DbSet<AnalyticsModel> Analytics { get; set; }
}