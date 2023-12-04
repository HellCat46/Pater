using System.ComponentModel.DataAnnotations;

namespace Web.Models.Link;

public class LinkModel
{
    [Key]
    public string code { get; set; }
    
    [Required]
    public string url { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateOnly CreatedAt { get; set; }
}