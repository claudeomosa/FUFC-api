using FUFC.Shared.Data;
using FUFC.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FUFC.Shared.Services;

public static class BoutServices
{
    public static IQueryable<object> GetAllBouts(UfcContext context)
    {
        IQueryable<Bout> bouts = context.Bouts
            .Include(b => b.RedCorner)
            .Include(b => b.BlueCorner)
            .Include(b => b.Event)
            .Include(b => b.Referee)
            .AsQueryable();
                
        var boutBases = bouts.AsNoTracking()
            .Select(bout => new 
                {
                    Id = bout.Id,
                    EventName = bout.Event.Name,
                    WeightClass = bout.WeightClass ?? string.Empty,
                    RedCornerFighter = new {
                        FighterId = bout.RedCorner.Id,
                        Name = bout.RedCorner.Name,
                        IsRanked = bout.RedCorner.IsRanked,                       
                    },
                    BlueCornerFighter = new 
                    { 
                        FighterId = bout.BlueCorner.Id,
                        Name = bout.BlueCorner.Name, 
                        IsRanked = bout.BlueCorner.IsRanked, 
                    }, 
                    RefereeName = bout.Referee.Name,
                }
            );
        return boutBases;
    }

    public static Bout? GetBoutById(UfcContext context, Ulid id)
    {
        return context.Bouts.FirstOrDefault(b => b.Id == id);
    }

    public static IQueryable<Bout> GetBoutsByEventId(UfcContext context, Ulid eventId)
    {
        return context.Bouts.Where(b => b.Event.Id == eventId);
    }

    public static IQueryable<Bout> GetBoutsByWeightClass(UfcContext context, string weightClass)
    {
        return context.Bouts.Where(b => b.WeightClass == weightClass);
    }

    public static Bout? GetBoutByFightersAndDate(UfcContext context, List<Fighter> corners, string weightClass, DateTime boutDate)
    {
        return context.Bouts.FirstOrDefault(
            b => b.WeightClass == weightClass && b.RedCorner == corners[0] && b.BlueCorner == corners[1] && b.Event.Date == boutDate);
    }
    
    public static void AddBout(UfcContext context, Bout bout)
    {
        context.Bouts.Add(bout);
        context.SaveChanges();
    }

    public static void UpdateBout(UfcContext context, Bout updatedBout)
    {
        var existingBout = context.Bouts.Find(updatedBout.Id);
        if (existingBout == null) return;

        existingBout.RedCorner = updatedBout.RedCorner;
        existingBout.BlueCorner = updatedBout.BlueCorner;
        existingBout.WeightClass = updatedBout.WeightClass;
        existingBout.IsForTitle = updatedBout.IsForTitle;
        existingBout.IsMainEvent = updatedBout.IsMainEvent;
        existingBout.IsPrelim = updatedBout.IsPrelim;
        existingBout.IsInMainCard = updatedBout.IsInMainCard;
        existingBout.Referee = updatedBout.Referee;
        existingBout.Result = updatedBout.Result;

        context.SaveChanges();
    }

    public static void DeleteBout(UfcContext context, Ulid id)
    {
        var boutToDelete = context.Bouts.Find(id);
        if (boutToDelete == null) return;

        context.Bouts.Remove(boutToDelete);
        context.SaveChanges();
    }
}