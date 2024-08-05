using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUFC.Shared.Models;

public class Bout
{
    [Key]
    public int Id { get; init; }

    public Event Event { get; init; } = new Event();
    
    public Fighter RedCorner { get; init; } = new Fighter();
    
    public Fighter BlueCorner { get; init; } = new Fighter();

    public bool IsForTitle { get; init; }
    
    public bool IsMainEvent  { get; init; }
    
    public bool IsPrelim { get; init; }
    
    public bool IsInMainCard { get; init; }
    
    public Referee Referee { get; init; } = new Referee();

    [Column(TypeName = "jsonb")]
    public BoutResult? Result { get; init; }
}

public class BoutResult
{
    [JsonPropertyName("winner")] public string Winner { get; init; } = string.Empty;

    [JsonPropertyName("round")] public int Round { get; init; }
    
    [JsonPropertyName("method")] public string Method { get; init; } = string.Empty;

}