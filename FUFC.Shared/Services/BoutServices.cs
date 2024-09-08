using FUFC.Shared.Data;
using FUFC.Shared.Models;

namespace FUFC.Shared.Services;

public static class BoutServices
{
    public static IQueryable<Bout> GetAllBouts(UfcContext context)
    {
        return from bout in context.Bouts select bout;
    }
    public static void AddBout(UfcContext context, Bout bout)
    {
        context.Bouts.Add(bout);
        context.SaveChanges();
    }
    public static Bout? GetBoutByFightersAndDate(UfcContext context, List<Fighter> corners, string weightClass, DateTime boutDate)
    {
        return context.Bouts.FirstOrDefault(
            b => b.WeightClass == weightClass && b.RedCorner == corners[0] && b.BlueCorner == corners[1] && b.Event.Date == boutDate);
    }
}