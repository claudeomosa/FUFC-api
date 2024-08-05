using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUFC.Shared.Models;

public class Bout(Event boutEvent)
{
    public Ulid Id { get; set; }
    
    public Fighter RedCorner { get; set; } = new Fighter(new WeightClass());
    
    public Fighter BlueCorner { get; set; } = new Fighter(new WeightClass());

    public bool IsForTitle { get; set; }
    
    public bool IsMainEvent  { get; set; }
    
    public Referee Referee { get; set; } = new Referee();

    [Column(TypeName = "jsonb")]
    public BoutResult? Result { get; init; } = boutEvent.Date > DateTime.UtcNow ? null : new BoutResult();
}

public class BoutResult
{
    [JsonPropertyName("winner")] public string Winner { get; set; } = string.Empty;

    [JsonPropertyName("round")] public int Round { get; set; }
    
    [JsonPropertyName("method")] public string Method { get; set; } = string.Empty;

}