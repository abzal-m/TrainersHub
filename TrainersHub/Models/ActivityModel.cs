using System.Text.Json.Serialization;

namespace TrainersHub.Models;

public class ActivityModel
{
    [JsonPropertyName("sport_type")]
    public string SportType { get; set; }
    
    [JsonPropertyName("distance")]
    public double Distance { get; set; }
    
    [JsonPropertyName("moving_time")]
    public double MovingTime { get; set; }
    
    [JsonPropertyName("total_elevation_gain")]
    public double TotalElevationGain { get; set; }
    
    [JsonPropertyName("average_speed")]
    public double AverageSpeed { get; set; }
    
    [JsonPropertyName("average_cadence")]
    public double AverageCadence { get; set; }
    
    [JsonPropertyName("average_heartrate")]
    public double AverageHeartrate { get; set; }
    
    [JsonPropertyName("max_heartrate")]
    public double MaxHeartrate { get; set; }
    
    [JsonPropertyName("start_date_local")]
    public DateTime StartDate { get; set; }
}