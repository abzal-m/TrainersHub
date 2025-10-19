using System.Text.Json.Serialization;

namespace TrainersHub.Models.Strava;

public class ActivityModel
{
    
    [JsonPropertyName("name")] public string Name { get; set; }
    
    [JsonPropertyName("athlete")]
    public Athlete Athlete { get; set; }
    
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

public class Athlete
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
}