using Microsoft.EntityFrameworkCore;
using Web.Models.Link;

namespace Web.ApplicationDbContext;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions options) : base(options) 
    {
        
    }
    
    public DbSet<LinkModel> Link { get; set; }
}