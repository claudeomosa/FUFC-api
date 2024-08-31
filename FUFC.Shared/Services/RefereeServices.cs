using FUFC.Shared.Data;
using FUFC.Shared.Models;

namespace FUFC.Shared.Services;

public class RefereeServices
{
    public static Referee Add(UfcContext context, Referee referee)
    {
        context.Add(referee);
        context.SaveChanges();
        return GetRefereeByName(context, referee.Name);
    }

    public static Referee? GetRefereeByName(UfcContext context, string refereeName)
    {
        return (from referee in context.Referees where referee.Name == refereeName select referee).FirstOrDefault();
    }
}