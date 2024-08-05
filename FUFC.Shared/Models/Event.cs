using System.ComponentModel.DataAnnotations;

namespace FUFC.Shared.Models;

public class Event
{
    [Key]
    public int Id { get; init; }

    public bool IsPpv { get; init; } = false;

    [MaxLength(50)]
    public string Venue { get; init; } = string.Empty;

    public ICollection<Bout> Bouts { get; init; } = new List<Bout>();

    public DateTime Date { get; init; }
}