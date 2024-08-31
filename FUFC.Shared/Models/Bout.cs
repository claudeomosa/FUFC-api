using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUFC.Shared.Models;

public class Bout
{
    [Key]
    public Ulid Id { get; set; }
    public Bout()
    {
        Id = Ulid.NewUlid();
    }
    [Required]
    public Event Event { get; set; }
    [Required]
    public Fighter RedCorner { get; set; }
    [Required]
    public Fighter BlueCorner { get; set; }
    public bool IsForTitle { get; set; }
    public bool IsMainEvent { get; set; }
    public bool IsPrelim { get; set; }
    public bool IsInMainCard { get; set; }
    [Required]
    public Referee Referee { get; set; }
    [Column(TypeName = "jsonb")]
    [Required]
    public BoutResult Result { get; set; }
}

public class BoutResult
{
    [JsonPropertyName("winner")] public string Winner { get; set; }
    [JsonPropertyName("round")] public int Round { get; set; }
    [JsonPropertyName("method")] public string Method { get; set; }
}