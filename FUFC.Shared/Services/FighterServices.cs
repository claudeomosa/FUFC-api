using FUFC.Shared.Data;
using FUFC.Shared.Models;

namespace FUFC.Shared.Services;

public static class FighterServices
{
    public static IQueryable<Fighter> GetAllFighters(UfcContext context)
    {
        return from fighter in context.Fighters select fighter;
    }
    public static void AddFighter(UfcContext context, Fighter fighter)
    {
        context.Fighters.Add(fighter);
        context.SaveChanges();
    }

    public static Fighter? GetFighterByNameAndNicknameInWeightClass(UfcContext context, string name, string nickname, string weightclass)
    {
        IQueryable<Fighter> fightersWithName = from fighter in context.Fighters where fighter.Name == name && fighter.WeightClass == weightclass select fighter;
        if (fightersWithName.Count() > 1)
        {
            return fightersWithName.FirstOrDefault(f => f.NickName == nickname);
        }
        else
        {
            return fightersWithName.FirstOrDefault();
        }
    }
    // Get all ranked fighters in a specific weight class, ordered by rank
    public static IQueryable<Fighter> GetRankedFightersInWeightClass(UfcContext context, string weightClass)
    {
        return context.Fighters
            .Where(f => f.WeightClass == weightClass && f.IsRanked && f.Active)
            .OrderBy(f => f.Rank);
    }

    // Get all champions (including interim)
    public static IQueryable<Fighter> GetAllChampions(UfcContext context)
    {
        return context.Fighters
            .Where(f => f.Champion || f.InterimChampion)
            .OrderBy(f => f.WeightClass);
    }

    // Get champion for a specific weight class
    public static Fighter? GetChampionInWeightClass(UfcContext context, string weightClass)
    {
        return context.Fighters
            .FirstOrDefault(f => f.WeightClass == weightClass && f.Champion);
    }

    // Get fighters by fighting style
    public static IQueryable<Fighter> GetFightersByStyle(UfcContext context, string predominantStyle)
    {
        return context.Fighters
            .Where(f => f.PredominantStyle == predominantStyle && f.Active)
            .OrderBy(f => f.Name);
    }

    // Get fighters within a specific win percentage range
    public static IQueryable<Fighter> GetFightersByWinPercentage(UfcContext context, double minPercentage, double maxPercentage)
    {
        return context.Fighters.Where(f => 
            f.Record != null && 
            f.Record.Wins + f.Record.Losses > 0 &&
            (double)f.Record.Wins / (f.Record.Wins + f.Record.Losses) * 100 >= minPercentage &&
            (double)f.Record.Wins / (f.Record.Wins + f.Record.Losses) * 100 <= maxPercentage)
            .OrderByDescending(f => (double)f.Record!.Wins / (f.Record.Wins + f.Record.Losses));
    }

    // Get fighters by striking accuracy threshold
    public static IQueryable<Fighter> GetFightersByStrikingAccuracy(UfcContext context, double minimumAccuracy)
    {
        return context.Fighters
            .Where(f => f.SkillStats != null && f.SkillStats.StrikingAccuracy >= minimumAccuracy)
            .OrderByDescending(f => f.SkillStats!.StrikingAccuracy);
    }

    // Update fighter's rank
    public static void UpdateFighterRank(UfcContext context, Ulid fighterId, int newRank)
    {
        var fighter = context.Fighters.Find(fighterId);
        if (fighter != null)
        {
            fighter.IsRanked = newRank > 0;
            fighter.Rank = newRank;
            context.SaveChanges();
        }
    }

    // Toggle fighter's active status
    public static void ToggleFighterActiveStatus(UfcContext context, Ulid fighterId)
    {
        var fighter = context.Fighters.Find(fighterId);
        if (fighter != null)
        {
            fighter.Active = !fighter.Active;
            if (!fighter.Active)
            {
                fighter.IsRanked = false;
                fighter.Champion = false;
                fighter.InterimChampion = false;
            }
            context.SaveChanges();
        }
    }

    // Get potential contenders (top 5 ranked fighters in a weight class)
    public static IQueryable<Fighter> GetTopContenders(UfcContext context, string weightClass, int count = 5)
    {
        return context.Fighters
            .Where(f => f.WeightClass == weightClass && f.IsRanked && !f.Champion && !f.InterimChampion && f.Active)
            .OrderBy(f => f.Rank)
            .Take(count);
    }

    // Search fighters by name or nickname
    public static IQueryable<Fighter> SearchFighters(UfcContext context, string searchTerm)
    {
        var normalizedSearch = searchTerm.ToLower();
        return context.Fighters
            .Where(f => f.Name.ToLower().Contains(normalizedSearch) || 
                       f.NickName.ToLower().Contains(normalizedSearch));
    }
}

