using FUFC.Shared.Data;
using FUFC.Shared.Models;
using FUFC.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FUFC.API.Controllers
{
    [Route("ufcapi/v1/[controller]")]
    [ApiController]
    public class BoutController : ControllerBase
    {
        private readonly UfcContext _context;

        /// <summary>
        /// Initializes the BoutController with the database context.
        /// </summary>
        /// <param name="context">The database context.</param>
        public BoutController(UfcContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all bouts from the database.
        /// </summary>
        /// <returns>A list of all bouts.</returns>
        [HttpGet]
        public async Task<ActionResult<List<object>>> GetBouts()
        {
            var bouts = BoutServices.GetAllBouts(_context);
            return Ok(bouts.ToList());
        }

        /// <summary>
        /// Retrieves a specific bout by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the bout.</param>
        /// <returns>The requested bout if found; otherwise, an error message.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Bout>> GetBout(Ulid id)
        {
            var bout = BoutServices.GetBoutById(_context, id);

            if (bout == null)
            {
                return NotFound(new { Message = $"Bout with ID {id} not found." });
            }

            return Ok(bout);
        }

        /// <summary>
        /// Creates a new bout and adds it to the database.
        /// </summary>
        /// <param name="bout">The bout to be created.</param>
        /// <returns>The created bout.</returns>
        [HttpPost]
        public async Task<ActionResult<Bout>> AddBout([FromBody] Bout bout)
        {
            try
            {
                if (bout == null)
                {
                    return BadRequest(new { Message = "Invalid bout data." });
                }

                BoutServices.AddBout(_context, bout);
                return CreatedAtAction(nameof(GetBout), new { id = bout.Id }, bout);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error adding bout: {ex.Message}" });
            }
        }

        /// <summary>
        /// Updates an existing bout.
        /// </summary>
        /// <param name="id">The ID of the bout to update.</param>
        /// <param name="updatedBout">The updated bout data.</param>
        /// <returns>The updated bout if successful.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<Bout>> UpdateBout(Ulid id, [FromBody] Bout updatedBout)
        {
            try
            {
                var bout = BoutServices.GetBoutById(_context, id);

                if (bout == null)
                {
                    return NotFound(new { Message = $"Bout with ID {id} not found." });
                }

                updatedBout.Id = id; // Ensure the ID matches
                BoutServices.UpdateBout(_context, updatedBout);

                return Ok(updatedBout);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error updating bout: {ex.Message}" });
            }
        }

        /// <summary>
        /// Deletes a specific bout by its ID.
        /// </summary>
        /// <param name="id">The ID of the bout to delete.</param>
        /// <returns>A success message if deletion is successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBout(Ulid id)
        {
            try
            {
                var bout = BoutServices.GetBoutById(_context, id);

                if (bout == null)
                {
                    return NotFound(new { Message = $"Bout with ID {id} not found." });
                }

                BoutServices.DeleteBout(_context, id);
                return Ok(new { Message = $"Bout with ID {id} deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error deleting bout: {ex.Message}" });
            }
        }
    }
}
