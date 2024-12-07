using FUFC.Shared.Data;
using FUFC.Shared.Models;
using FUFC.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FUFC.API.Controllers;

[Route("ufcapi/v1/[controller]")]
[ApiController]
public class FightersController : ControllerBase
{
    private readonly UfcContext _db;

    public FightersController(UfcContext context)
    {
        _db = context;
    }

    [HttpGet("",Name = "GetAllFighters")]
    public ActionResult<IEnumerable<Fighter>> GetFighters()
    {
        try
        {
            var fighters = FighterServices.GetAllFighters(_db).ToList();
            return Ok(fighters);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while retrieving fighters");
        }
    }

    [HttpGet("{id}", Name = "GetFighter")]
    public ActionResult<Fighter> GetFighter(Ulid id)
    {
        try
        {
            var fighter = _db.Fighters.Find(id);
            if (fighter == null)
            {
                return NotFound($"Fighter with ID {id} not found");
            }
            return Ok(fighter);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving fighter with ID {id}");
        }
    }

    [HttpGet("ranked/{weightClass}", Name = "GetRankedFighters")]
    public ActionResult<IEnumerable<Fighter>> GetRankedFighters(string weightClass)
    {
        try
        {
            var rankedFighters = FighterServices.GetRankedFightersInWeightClass(_db, weightClass).ToList();
            return Ok(rankedFighters);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving ranked fighters for {weightClass}");
        }
    }

    [HttpGet("champions", Name = "GetAllChampions")]
    public ActionResult<IEnumerable<Fighter>> GetChampions()
    {
        try
        {
            var champions = FighterServices.GetAllChampions(_db).ToList();
            return Ok(champions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while retrieving champions");
        }
    }

    [HttpGet("champion/{weightClass}", Name = "GetChampion")]
    public ActionResult<Fighter> GetChampion(string weightClass)
    {
        try
        {
            var champion = FighterServices.GetChampionInWeightClass(_db, weightClass);
            if (champion == null)
            {
                return NotFound($"No champion found for {weightClass}");
            }
            return Ok(champion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving champion for {weightClass}");
        }
    }

    [HttpGet("style/{style}", Name = "GetFightersByStyle")]
    public ActionResult<IEnumerable<Fighter>> GetFightersByStyle(string style)
    {
        try
        {
            var fighters = FighterServices.GetFightersByStyle(_db, style).ToList();
            return Ok(fighters);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving fighters with style {style}");
        }
    }

    [HttpGet("winrate/{minPercentage}/{maxPercentage}", Name = "GetFightersByWinPercentage")]
    public ActionResult<IEnumerable<Fighter>> GetFightersByWinPercentage(double minPercentage, double maxPercentage)
    {
        try
        {
            if (minPercentage < 0 || maxPercentage > 100 || minPercentage > maxPercentage)
            {
                return BadRequest("Invalid percentage range");
            }

            var fighters = FighterServices.GetFightersByWinPercentage(_db, minPercentage, maxPercentage).ToList();
            return Ok(fighters);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while retrieving fighters by win percentage");
        }
    }

    [HttpGet("striking/{minimumAccuracy}", Name = "GetFightersByStrikingAccuracy")]
    public ActionResult<IEnumerable<Fighter>> GetFightersByStrikingAccuracy(double minimumAccuracy)
    {
        try
        {
            if (minimumAccuracy < 0 || minimumAccuracy > 100)
            {
                return BadRequest("Invalid accuracy value");
            }

            var fighters = FighterServices.GetFightersByStrikingAccuracy(_db, minimumAccuracy).ToList();
            return Ok(fighters);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while retrieving fighters by striking accuracy");
        }
    }

    [HttpGet("contenders/{weightClass}/{count}", Name = "GetTopContenders")]
    public ActionResult<IEnumerable<Fighter>> GetTopContenders(string weightClass, int count = 5)
    {
        try
        {
            var contenders = FighterServices.GetTopContenders(_db, weightClass, count).ToList();
            return Ok(contenders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving top contenders for {weightClass}");
        }
    }

    [HttpGet("search/{searchTerm}", Name = "SearchFighters")]
    public ActionResult<IEnumerable<Fighter>> SearchFighters(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term cannot be empty");
            }

            var fighters = FighterServices.SearchFighters(_db, searchTerm).ToList();
            return Ok(fighters);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while searching for fighters");
        }
    }

    [HttpPost(Name = "AddFighter")]
    public ActionResult<Fighter> AddFighter(Fighter fighter)
    {
        try
        {
            if (fighter == null)
            {
                return BadRequest("Fighter data is required");
            }

            FighterServices.AddFighter(_db, fighter);
            return CreatedAtRoute("GetFighter", new { id = fighter.Id }, fighter);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while adding the fighter");
        }
    }

    [HttpPut("rank/{id}", Name = "UpdateFighterRank")]
    public ActionResult UpdateFighterRank(Ulid id, [FromBody] int newRank)
    {
        try
        {
            if (newRank < 0 || newRank > 15)
            {
                return BadRequest("Invalid rank value");
            }

            FighterServices.UpdateFighterRank(_db, id, newRank);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while updating fighter rank");
        }
    }

    [HttpPut("toggle-active/{id}", Name = "ToggleFighterActiveStatus")]
    public ActionResult ToggleFighterActiveStatus(Ulid id)
    {
        try
        {
            FighterServices.ToggleFighterActiveStatus(_db, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while toggling fighter active status");
        }
    }

    [HttpGet("by-name/{name}/{nickname}/{weightClass}", Name = "GetFighterByNameAndNickname")]
    public ActionResult<Fighter> GetFighterByNameAndNickname(string name, string nickname, string weightClass)
    {
        try
        {
            var fighter = FighterServices.GetFighterByNameAndNicknameInWeightClass(_db, name, nickname, weightClass);
            if (fighter == null)
            {
                return NotFound($"Fighter not found with name {name} and nickname {nickname} in {weightClass}");
            }
            return Ok(fighter);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while retrieving the fighter");
        }
    }
}


