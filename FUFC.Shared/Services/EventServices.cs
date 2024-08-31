using FUFC.Shared.Data;
using FUFC.Shared.Models;

namespace FUFC.Shared.Services;

public static class EventServices
{
    public static IQueryable<Event> GetAllEvents(UfcContext context)
    {
        return from e in context.Events select e;
    }

    public static void AddEvent(UfcContext context, Event e)
    {
        if (e.Date.Kind == DateTimeKind.Unspecified)
        {
            e.Date = DateTime.SpecifyKind(e.Date, DateTimeKind.Utc);
        }
        else
        {
            e.Date = e.Date.ToUniversalTime();
        }
        context.Events.Add(e);
        context.SaveChanges();
    }
}