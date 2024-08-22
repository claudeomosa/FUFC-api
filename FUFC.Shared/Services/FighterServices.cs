using FUFC.Shared.Data;
using FUFC.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FUFC.Shared.Services;

public class FighterServices()
{
    /*
    public static IQueryable<Fighter> GetFighters()
    {
        using (var dbContext = new UfcContext(_options))
        {
            return from fighter in dbContext.Fighters select fighter;
        }

    }
*/
    public static void AddFighter(UfcContext context, Fighter fighter)
    {
        context.Fighters.Add(fighter);
        context.SaveChanges();
    }
}