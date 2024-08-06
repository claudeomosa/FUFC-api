using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUFC.Shared.Models;

public class Fighter
{
    [Key]
    public int Id { get; init; }
    
    [MaxLength(50)]
    public string Name { get; init; } = string.Empty;
    
    [MaxLength(50)]
    public string NickName { get; init; } = string.Empty;

    public string WeightClass { get; init; } = Models.WeightClass.Unknown;

    public string PredominantStyle { get; init; } = FightingStyle.Unknown;
    
    [Column(TypeName = "jsonb")]
    public FighterRecord? Record { get; init; }

    public bool Champion { get; init; }
    
    public bool InterimChampion { get; init; }
    
    public double Height { get; init; }
    
    public double Weight { get; init; }

    [MaxLength(50)]
    public string HomeCity { get; init; } = string.Empty;

    public bool Active { get; init; } = true;

    private readonly bool _isRanked;
    public bool IsRanked 
    { 
        get => _isRanked; 
        init 
        { 
            _isRanked = value; 
            if (!_isRanked)
            {
                Rank = 0;
            }
        } 
    }

    private readonly int _rank;
    public int Rank 
    { 
        get => _rank; 
        init => _rank = IsRanked ? value : 0;
    }

    [Column(TypeName = "jsonb")]
    public FighterSkillStats SkillStats { get; init; } = new FighterSkillStats();
    
    public double Reach { get; init;  }

    [MaxLength(10)]
    public string Gender { get; init; } = string.Empty;
    
    public string Stance { get; init; } = FightingStance.Unknown;
    
    public int Age { get; init; }
    
    [Column(TypeName = "jsonb")]
    public SocialMedia SocialMedia { get; init; } = new SocialMedia();

    public Gym Gym { get; init; } = new Gym();
}
public class SocialMedia
{
    public string Twitter { get; init; } = string.Empty;
    public string Facebook { get; init; } = string.Empty;
    public string Instagram { get; init; } = string.Empty;
}
public class FighterRecord
{
    [JsonPropertyName("wins")]
    public int Wins { get; init; }

    [JsonPropertyName("losses")]
    public int Losses { get; init; }

    [JsonPropertyName("draws")]
    public int Draws { get; init; }

    [JsonPropertyName("no_contests")]
    public int NoContests { get; init; }
    
    [JsonPropertyName("wins_by_ko_or_tko")]
    public int WinsByKoOrTko { get; init; }
    
    [JsonPropertyName("wins_by_sub")]
    public int WinsBySub { get; init; }
    
    [JsonPropertyName("wins_by_decision")]
    public int WinsByDecision { get; init; }
    
    [JsonPropertyName("losses_by_ko_or_tko")]
    public int LossesByKoOrTko { get; init; }
    
    [JsonPropertyName("losses_by_sub")]
    public int LossesBySub { get; init; }
    
    [JsonPropertyName("losses_by_decision")]
    public int LossesByDecision { get; init; }
}

public class FighterSkillStats
{
    [JsonPropertyName("striking_accuracy")]
    public double StrikingAccuracy { get; init; }
    
    [JsonPropertyName("striking_defense")]
    public double StrikingDefense { get; init; }
    
    [JsonPropertyName("takedown_average")]
    public double TakedownAverage { get; init; }
    
    [JsonPropertyName("takedown_defense")]
    public double TakedownDefense { get; init; }

    [JsonPropertyName("average_fight_time")]
    public TimeSpan AverageFightTime { get; init; }

    [JsonPropertyName("submission_average")]
    public double SubmissionAverage { get; init; }
    
    [JsonPropertyName("average_strikes_landed_per_minute")]
    public double AverageStrikesLandedPerMinute { get; init; }
    
    [JsonPropertyName("average_strikes_absorbed_per_minute")]
    public double AverageStrikesAbsorbedPerMinute { get; init; }
}