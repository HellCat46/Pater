using System.Text.Json.Serialization;

namespace Web.Models.View;

public record class VisitorGeoLoc
{
    public string status { get; set; } 
    public string country { get; set; } 
    public string city { get; set; }
}