using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUFC.Shared.Models;

public class Bout
{
    [Key]
    public int Id { get; set; }

    public Event Event { get; set; } = new Event();

    public Fighter RedCorner { get; set; } = new Fighter();

    public Fighter BlueCorner { get; set; } = new Fighter();

    public bool IsForTitle { get; set; }

    public bool IsMainEvent { get; set; }

    public bool IsPrelim { get; set; }

    public bool IsInMainCard { get; set; }

    public Referee Referee { get; set; } = new Referee();

    [Column(TypeName = "jsonb")]
    public BoutResult? Result { get; set; }
}

public class BoutResult
{
    [JsonPropertyName("winner")] public string Winner { get; set; } = string.Empty;

    [JsonPropertyName("round")] public int Round { get; set; }

    [JsonPropertyName("method")] public string Method { get; set; } = string.Empty;

}