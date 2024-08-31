using System.ComponentModel.DataAnnotations;

namespace FUFC.Shared.Models;

public class Event
{
    [Key]
    public Ulid Id { get; set; }

    public Event()
    {
        Id = Ulid.NewUlid();
    }
    public bool IsPpv { get; set; }

    [MaxLength(150)]
    public string Venue { get; set; } = string.Empty;

    private string _name = string.Empty;

    [MaxLength(150)]
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            IsPpv = IsPayPerViewEvent(value);
        }
    }

    public ICollection<Bout> Bouts { get; set; }

    public DateTime Date { get; set; }

    public string FormattedDate
    {
        get
        {
            return Date.ToString("MMMM-dd-yyyy");
        }
    }

    private bool IsPayPerViewEvent(string eventName)
    {
        if (eventName.StartsWith("UFC") && eventName.Length > 3)
        {
            string remainder = eventName.Substring(3).Trim();

            if (char.IsDigit(remainder[0]))
            {
                return true;
            }
        }
        return false;
    }
}