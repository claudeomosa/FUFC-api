using FUFC.Shared.Data;
using FUFC.Shared.Models;

namespace FUFC.Shared.Services;

public static class GymServices
{
    public static IQueryable<Gym> GetAllGyms(UfcContext context)
    {
        return from gym in context.Gyms select gym;
    }

    public static Gym? GetGymByName(UfcContext context, string name)
    {
        return (from gym in context.Gyms where gym.Name == name select gym).FirstOrDefault();
    }

    public static void AddGym(UfcContext context, Gym gym)
    {
        context.Gyms.Add(gym);
        context.SaveChanges();
    }
}