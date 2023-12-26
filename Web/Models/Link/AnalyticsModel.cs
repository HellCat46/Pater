using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Primitives;
using UAParser;

namespace Web.Models.Link;

public class AnalyticsModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    
    [Required]
    public Browser browser { get; set; }
    
    [Required]
    public Device device { get; set; }
    
    [Required]
    public OperatingSystem os { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string country { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string city { get; set; }
    
    [Required]
    public DateTime visitedAt { get; set; }
    
    
    public string LinkModelCode { get; set; }
    public LinkModel LinkModel { get; set; }

    public enum Device
    {
        Phone,
        Desktop,
        Other
    }
    public enum Browser
    {
        Firefox,
        Chromium,
        Safari,
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

    public static (Browser, Device, OperatingSystem) ParseUserAgent(StringValues userAgent)
    {
        Browser browser = Browser.Others;
        OperatingSystem os = OperatingSystem.Others;
        Device device = Device.Other;

        Parser parser = Parser.GetDefault();
        ClientInfo clientInfo = parser.Parse(userAgent.ToString());

        string userBrowser = clientInfo.UA.Family;
        if (userBrowser.Contains("Safari")) browser = Browser.Safari;
        else if (userBrowser.Contains("Chrome")) browser = Browser.Chromium;
        else if (userBrowser.Contains("Firefox")) browser = Browser.Firefox;

        string userOS = clientInfo.OS.Family;
        if (userOS.Contains("Android"))
        {
            device = Device.Phone;
            os = OperatingSystem.Android;
        }else if (userOS.Contains("Linux"))
        {
            device = Device.Desktop;
            os = OperatingSystem.Linux;
        }else if (userOS.Contains("Windows"))
        {
            device = Device.Desktop;
            os = OperatingSystem.Windows;
        }else if (userOS.Contains("iOS"))
        {
            device = Device.Phone;
            os = OperatingSystem.IOS;
        }else if (userOS.Contains("Mac"))
        {
            device = Device.Phone;
            os = OperatingSystem.Mac;
        }
        
        
        
        return (browser,device, os);
    } 
}