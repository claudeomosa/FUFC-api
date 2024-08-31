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
}