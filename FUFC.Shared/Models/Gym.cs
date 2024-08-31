using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUFC.Shared.Models;

public class Gym
{
    [Key]
    public Ulid Id { get; init; }

    public Gym()
    {
        Id = new Ulid();
    }

    [MaxLength(50)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(50)]
    public string Location { get; init; } = string.Empty;

    public List<Fighter> Fighters { get; init; }

    [MaxLength(50)]
    public string HeadCoach { get; init; } = String.Empty;

    public string IsGoodFor { get; init; } = FightingStyle.Unknown;
}