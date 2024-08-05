namespace FUFC.Shared.Models;

public class Event
{
    public Ulid Id { get; set; }

    public bool IsPpv { get; set; } = false;

    public string Venue { get; set; } = string.Empty;

    public ICollection<Bout> PrelimCard { get; set; } = new List<Bout>();

    public ICollection<Bout> MainCard { get; set; } = new List<Bout>();
    public DateTime Date { get; set; }
}