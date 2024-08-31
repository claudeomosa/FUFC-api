using System.ComponentModel.DataAnnotations;

namespace FUFC.Shared.Models;

public class Referee
{
    [Key]
    public Ulid Id { get; init; }

    public Referee()
    {
        Id = Ulid.NewUlid();
    }

    public string Name { get; init; } = string.Empty;
}