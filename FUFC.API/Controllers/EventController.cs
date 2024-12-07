using FUFC.Shared.Data;
using FUFC.Shared.Models;
using FUFC.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FUFC.API.Controllers;

[Route("ufcapi/v1/[controller]")]
[ApiController]
public class EventController : ControllerBase
{
    private readonly UfcContext _context;

    /// <summary>
    /// Initializes the EventController with the database context.
    /// </summary>
    /// <param name="context">The database context.</param>
    public EventController(UfcContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all events from the database.
    /// </summary>
    /// <returns>A list of all events.</returns>
    [HttpGet]
    public async Task<ActionResult<List<Event>>> GetEvents()
    {
        var events = EventServices.GetAllEvents(_context).ToList();
        return Ok(events);
    }

    /// <summary>
    /// Retrieves a specific event by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the event.</param>
    /// <returns>The requested event if found; otherwise, an error message.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetEvent(Ulid id)
    {
        var eventItem = EventServices.GetEventById(_context, id);

        if (eventItem == null)
        {
            return NotFound(new { Message = $"Event with ID {id} not found." });
        }

        return Ok(eventItem);
    }

    /// <summary>
    /// Retrieves events within a specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A list of events within the specified date range.</returns>
    [HttpGet("date-range")]
    public async Task<ActionResult<List<Event>>> GetEventsByDateRange(DateTime startDate, DateTime endDate)
    {
        var events = EventServices.GetEventsByDateRange(_context, startDate, endDate).ToList();
        return Ok(events);
    }

    /// <summary>
    /// Creates a new event and adds it to the database.
    /// </summary>
    /// <param name="eventItem">The event to be created.</param>
    /// <returns>The created event.</returns>
    [HttpPost]
    public async Task<ActionResult<Event>> AddEvent([FromBody] Event eventItem)
    {
        try
        {
            if (eventItem == null)
            {
                return BadRequest(new { Message = "Invalid event data." });
            }

            EventServices.AddEvent(_context, eventItem);
            return CreatedAtAction(nameof(GetEvent), new { id = eventItem.Id }, eventItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = $"Error adding event: {ex.Message}" });
        }
    }

    /// <summary>
    /// Updates an existing event.
    /// </summary>
    /// <param name="id">The ID of the event to update.</param>
    /// <param name="updatedEvent">The updated event data.</param>
    /// <returns>The updated event if successful.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<Event>> UpdateEvent(Ulid id, [FromBody] Event updatedEvent)
    {
        try
        {
            var eventItem = EventServices.GetEventById(_context, id);

            if (eventItem == null)
            {
                return NotFound(new { Message = $"Event with ID {id} not found." });
            }

            updatedEvent.Id = id; // Ensure the ID matches
            EventServices.UpdateEvent(_context, updatedEvent);

            return Ok(updatedEvent);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = $"Error updating event: {ex.Message}" });
        }
    }

    /// <summary>
    /// Deletes a specific event by its ID.
    /// </summary>
    /// <param name="id">The ID of the event to delete.</param>
    /// <returns>A success message if deletion is successful.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(Ulid id)
    {
        try
        {
            var eventItem = EventServices.GetEventById(_context, id);

            if (eventItem == null)
            {
                return NotFound(new { Message = $"Event with ID {id} not found." });
            }

            EventServices.DeleteEvent(_context, id);
            return Ok(new { Message = $"Event with ID {id} deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = $"Error deleting event: {ex.Message}" });
        }
    }
}