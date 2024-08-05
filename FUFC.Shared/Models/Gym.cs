namespace FUFC.Shared.Models;

public class Gym
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public List<Fighter> Fighters { get; set; } = new List<Fighter>();
    
    public string HeadCoach { get; set; } = String.Empty;
    
    public FightingStyle IsGoodFor { get; set; }
}