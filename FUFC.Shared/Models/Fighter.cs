using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUFC.Shared.Models;

public class Fighter
{
    [Key]
    public Ulid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public Fighter()
    {
        Id = Ulid.NewUlid();
        CreatedAt = DateTime.UtcNow;
    }

    [MaxLength(50)]
    [Required]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string NickName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string WeightClass { get; set; } = Models.WeightClass.Unknown;

    [MaxLength(30)]
    public string PredominantStyle { get; set; } = FightingStyle.Unknown;

    [Column(TypeName = "jsonb")]
    [Required]
    public FighterRecord? Record { get; set; }

    public bool Champion { get; set; }

    public bool InterimChampion { get; set; }

    public double Height { get; set; }

    // Read-only property to get height in feet and inches
    [NotMapped]
    public string HeightInFeet
    {
        get
        {
            double totalInches = Height / 2.54; // Convert cm to inches
            int feet = (int)(totalInches / 12);
            int inches = (int)(totalInches % 12);
            return $"{feet}'{inches}\"";
        }
    }
    public int Weight { get; set; }

    [MaxLength(50)]
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
    public FighterSkillStats? SkillStats { get; set; }

    public double Reach { get; set; }

    [MaxLength(10)] public string Gender { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Stance { get; set; } = FightingStance.Unknown;

    public int Age { get; set; }

    [Column(TypeName = "jsonb")]
    public SocialMedia? SocialMedia { get; set; }

    public Gym? Gym { get; set; }
}
public class SocialMedia
{
    [MaxLength(30)]
    public string Twitter { get; set; } = string.Empty;
    [MaxLength(30)]
    public string Facebook { get; set; } = string.Empty;
    [MaxLength(30)]
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
    [JsonPropertyName("takedown_accuracy")]
    public double TakedownAccuracy { get; set; }
    [JsonPropertyName("average_fight_time")]
    public TimeSpan AverageFightTime { get; set; }
    [JsonPropertyName("submission_average")]
    public double SubmissionAverage { get; set; }
    [JsonPropertyName("average_strikes_landed_per_minute")]
    public double AverageStrikesLandedPerMinute { get; set; }
    [JsonPropertyName("average_strikes_absorbed_per_minute")]
    public double AverageStrikesAbsorbedPerMinute { get; set; }
}