using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUFC.Shared.Models;

public class Gym
{
    [Key]
    public int Id { get; init; }

    [MaxLength(50)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(50)]
    public string Location { get; init; } = string.Empty;

    public List<Fighter> Fighters { get; init; } = new List<Fighter>();
    
    [MaxLength(50)]
    public string HeadCoach { get; init; } = String.Empty;
    
    [NotMapped]
    public FightingStyle? IsGoodFor { get; init; }
}