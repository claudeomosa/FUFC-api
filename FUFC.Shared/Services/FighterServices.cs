using FUFC.Shared.Common;
using FUFC.Shared.Data;
using FUFC.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
}