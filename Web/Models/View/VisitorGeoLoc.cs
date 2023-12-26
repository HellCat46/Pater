using System.Text.Json.Serialization;

namespace Web.Models.View;

public class VisitorGeoLoc
{
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonPropertyName("country")]
    public string Country { get; set; }
    
    [JsonPropertyName("city")]
    public string City { get; set; }
}