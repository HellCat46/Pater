using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web.Models.Link;

namespace Web.Models;

public class AnalyticsModel
{
    [Required]
    public Device device { get; set; }
    
    [Required]
    public OperatingSystem os { get; set; }
    
    [Required]
    public string country { get; set; }
    
    [Required]
    public string city { get; set; }
    
    [Required]
    public DateTime visitedAt { get; set; }
    
    
    [Required]
    [ForeignKey("LinkCode")]
    public string LinkCode { get; set; }
    public LinkModel LinkModel { get; set; }

    public enum Device
    {
        Desktop,
        Mobile,
        Others
    }

    public enum OperatingSystem
    {
        Linux,
        Windows,
        Mac,
        Android,
        IOS,
        Others
    }
}