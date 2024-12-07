using FUFC.Shared.Data;
using FUFC.Shared.Models;

namespace FUFC.Shared.Services;

public static class EventServices
{
    public static IQueryable<Event> GetAllEvents(UfcContext context)
    {
        return context.Events.AsQueryable();
    }

    public static Event? GetEventById(UfcContext context, Ulid id)
    {
        return context.Events.FirstOrDefault(e => e.Id == id);
    }

    public static IQueryable<Event> GetEventsByDateRange(UfcContext context, DateTime startDate, DateTime endDate)
    {
        return context.Events.Where(e => e.Date >= startDate && e.Date <= endDate);
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

    public static void UpdateEvent(UfcContext context, Event updatedEvent)
    {
        var existingEvent = context.Events.Find(updatedEvent.Id);
        if (existingEvent == null) return;

        existingEvent.Name = updatedEvent.Name;
        existingEvent.Venue = updatedEvent.Venue;
        existingEvent.Date = updatedEvent.Date;
        context.SaveChanges();
    }

    public static void DeleteEvent(UfcContext context, Ulid id)
    {
        var eventToDelete = context.Events.Find(id);
        if (eventToDelete == null) return;

        context.Events.Remove(eventToDelete);
        context.SaveChanges();
    }
}