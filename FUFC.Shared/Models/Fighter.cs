using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUFC.Shared.Models;

public class Fighter(WeightClass fighterWeightClass)
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string NickName { get; set; } = string.Empty;

    public WeightClass WeightClass { get; init; } = fighterWeightClass;

    public FightingStyle? PredominantStyle { get; set; }
    
    [Column(TypeName = "jsonb")]
    public FighterRecord Record { get; set; } = new FighterRecord();

    public bool Champion { get; set; } = false;
    
    public bool InterimChampion { get; set; } = false;
    
    public double Height { get; set; }
    
    public double Weight { get; set; }

    public string HomeCity { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    private bool _isRanked;
    public bool IsRanked 
    { 
        get => _isRanked; 
        set 
        { 
            _isRanked = value; 
            if (!_isRanked)
            {
                Rank = 0;
            }
        } 
    }

    private int _rank;
    public int Rank 
    { 
        get => _rank; 
        set => _rank = IsRanked ? value : 0;
    }

    [Column(TypeName = "jsonb")]
    public FighterSkillStats SkillStats { get; set; } = new FighterSkillStats();
    
    public double Reach { get; set;  }
    
    public FightingStance? Stance { get; set; }
    
    public int Age { get; set; }
    
    [Column(TypeName = "jsonb")]
    public SocialMedia SocialMedia { get; set; } = new SocialMedia();

    public Gym Gym { get; set; } = new Gym();
}
public class SocialMedia
{
    public string Twitter { get; set; } = string.Empty;
    public string Facebook { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
}
public class FighterRecord
{
    [JsonPropertyName("wins")]
    public int Wins { get; set; }

    [JsonPropertyName("losses")]
    public int Losses { get; set; }

    [JsonPropertyName("draws")]
    public int Draws { get; set; }

    [JsonPropertyName("no_contests")]
    public int NoContests { get; set; }
    
    [JsonPropertyName("wins_by_ko_or_tko")]
    public int WinsByKoOrTko { get; set; }
    
    [JsonPropertyName("wins_by_sub")]
    public int WinsBySub { get; set; }
    
    [JsonPropertyName("wins_by_decision")]
    public int WinsByDecision { get; set; }
    
    [JsonPropertyName("losses_by_ko_or_tko")]
    public int LossesByKoOrTko { get; set; }
    
    [JsonPropertyName("losses_by_sub")]
    public int LossesBySub { get; set; }
    
    [JsonPropertyName("losses_by_decision")]
    public int LossesByDecision { get; set; }
}

public class FighterSkillStats
{
    [JsonPropertyName("striking_accuracy")]
    public double StrikingAccuracy { get; set; }
    
    [JsonPropertyName("striking_defense")]
    public double StrikingDefense { get; set; }
    
    [JsonPropertyName("takedown_average")]
    public double TakedownAverage { get; set; }
    
    [JsonPropertyName("takedown_defense")]
    public double TakedownDefense { get; set; }

    [JsonPropertyName("average_fight_time")]
    public TimeSpan AverageFightTime { get; set; }

    [JsonPropertyName("submission_average")]
    public double SubmissionAverage { get; set; }
    
    [JsonPropertyName("average_strikes_landed_per_minute")]
    public double AverageStrikesLandedPerMinute { get; set; }
    
    [JsonPropertyName("average_strikes_absorbed_per_minute")]
    public double AverageStrikesAbsorbedPerMinute { get; set; }
}