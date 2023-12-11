using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web.Models.Link;

namespace Web.Models.Account;

public class AccountModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(Order = 0)]
    public int id { get; set; }
    
    [Required]
    [Column(Order = 1, TypeName = "VARCHAR")]
    [StringLength(255)]
    public string email { get; set; }
    
    [Required]
    [DefaultValue(false)]
    [Column(Order = 5)]
    public bool isAdmin { get; set; }
    
    [Required]
    [DefaultValue(false)]
    [Column(Order = 6)]
    public bool isVerified { get; set; }
    
    [Required]
    [DefaultValue(Plans.Free)]
    public Plans Plan { get; set; }
    
    [Required]
    public DateTime createdAt { get; set; }
    
    [Required]
    [Column(Order = 3, TypeName = "VARCHAR")]
    [StringLength(20)]
    public string name { get; set; }
    
    [Column(Order = 2, TypeName = "VARCHAR")]
    [StringLength(15)]
    public string? password { get; set; } 
    
    
    [Column(Order = 4)]
    [StringLength(41)]
    public string? PicPath { get; set; }
    
    public ExternalAuthModel ExternalAuth { get; set; }
    
    public ICollection<LinkModel> Links { get; set; }
    
    public ICollection<ActivityLogsModel> Logs { get; set; }

    public enum Plans
    {
        Free,
        Premium,
        Business,
        Custom
    }
}