using System.ComponentModel.DataAnnotations;

namespace Web.Models.Link;

public class IndexView
{
    
    [Required]
    public string url { get; set; }
    
    public bool created { get; set; }
    public string message { get; set; }
}