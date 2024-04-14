using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MongoDB.Driver;

using Mongotest.Data;
using Mongotest.Models.V1;

using System.Text.Json;

namespace Mongotest.Controllers.V1
{
    //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Route("api/V1/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly IApplicationDA _db;
        private readonly ILogger<PersonController> _logger;
        private readonly ApplicationEFContext _context;

        public PersonController(IApplicationDA db, ILogger<PersonController> logger)
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            _context = ApplicationEFContext.Create(mongoClient.GetDatabase("mongotest_v1_ef"));
            _db = db; 
            _logger = logger;
        }
        // GET: api/<PersonController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonModel>>> Get([FromQuery] bool useEF)
        {
            try
            {
                if (useEF)
                {
                    var result = _context.People;
                    return Ok(result);
                }
                return Ok(await _db.GetAllAsync<PersonModel>(nameof(PersonModel)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all people");
                return BadRequest(ex.Message);
            }
        }

        // GET api/<PersonController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PersonModel?>> Get(Guid id, [FromQuery] bool useEF)
        {
            try
            {
                if (useEF)
                {
                    return Ok(await _context.People.FindAsync(id));
                }
                return Ok(await _db.GetOneAsync<PersonModel>(id, nameof(PersonModel)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting one PeopleModel");
                return BadRequest(ex.Message);
            }
        }
        // POST api/<PersonController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PersonModel person, [FromQuery] bool useEF)
        {
            PersonModel? pef = default;
            try
            {
                if (useEF)
                {
                    pef = new PersonModel
                    {
                        Id = Guid.NewGuid(),
                        Name = person.Name,
                        Age = person.Age,
                        IsHuman = person.IsHuman
                    };
                    await _context.People.AddAsync(pef);
                    await _context.SaveChangesAsync();
                    return Ok(pef);
                }
                //person.Id = Guid.NewGuid().ToString();
                await _db.CreateAsync(person, nameof(PersonModel));
                return Ok(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PersonModel record");
                return BadRequest(ex.Message);
            }finally
            {
                if (pef is not null)
                {
                    AddHistoryEF(pef.Id, pef, HistoryAction.Created);
                }
                _logger.LogInformation("Person created");
            }
        }
        // PUT api/<PersonController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] PersonModel person, [FromQuery] bool useEF)
        {
            PersonModel? personToBeUpdated = default;
            try
            {
                if(id != person.Id)
                {
                    return BadRequest("Ambiguous identity");
                }
                if (useEF)
                {
                    personToBeUpdated = await _context.People.FindAsync(id);
                    if (personToBeUpdated == null)
                    {
                        return NotFound();
                    }
                    personToBeUpdated.Name = person.Name;
                    personToBeUpdated.Age = person.Age;
                    personToBeUpdated.IsHuman = person.IsHuman;
                    _context.People.Update(personToBeUpdated);
                    await _context.SaveChangesAsync();
                    return Accepted();
                }
                await _db.UpsertAsync(id, person, nameof(PersonModel));
                return Accepted();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Person");
                return BadRequest(ex.Message);
            }finally
            {
                if (personToBeUpdated is not null)
                {
                    AddHistoryEF(id, personToBeUpdated, HistoryAction.Updated);
                }
                _logger.LogInformation("Person updated");
            }
        }
        // DELETE api/<PersonController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id, [FromQuery] bool useEF)
        {
            PersonModel? personToBeDeleted = default;
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Invalid");
                }
                if (useEF)
                {
                    personToBeDeleted = await _context.People.FindAsync(id);
                    if (personToBeDeleted == null)
                    {
                        return NotFound();
                    }
                    _context.People.Remove(personToBeDeleted);
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                await _db.DeleteAsync<PersonModel>(id, nameof(PersonModel));
                return NoContent();
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error deleting Person");
                return BadRequest(ex.Message);
            }finally
            {
                if (personToBeDeleted is not null)
                {
                    AddHistoryEF(id, personToBeDeleted, HistoryAction.Deleted);
                }
                _logger.LogInformation("Person deleted");
            }
        }
        private async void AddHistoryEF(Guid id, PersonModel model, HistoryAction action = HistoryAction.Created)
        {
            // First check if there is a history for the model
            var historyModel = _context.Histories.SingleOrDefault(x => x.ModelId == model.Id);
            if (historyModel is not null)
            {
                historyModel.DateLastUpdated = DateTime.UtcNow;

                var json = JsonSerializer.Serialize(model);
                var list = historyModel.HistoryEntries.ToList();
                var item = CreateHistoryItem(model.GetType().Name, json, historyModel.Id, action);          
                _context.HistoryItems.Add(item);
                await _context.SaveChangesAsync();
            }
            else
            {
                var history = new HistoryModelEF
                {
                    Id = Guid.NewGuid(),
                    ModelId = id,
                    Notes = "",
                    DateLastUpdated = DateTime.UtcNow,
                    DateCreated = DateTime.UtcNow
                };
                if (history.HistoryEntries is null)
                {
                    history.HistoryEntries = new();
                }
                var json = JsonSerializer.Serialize(model);
                var item = CreateHistoryItem(model.GetType().Name, json, history.Id, action);
                _context.Histories.Add(history);
                _context.HistoryItems.Add(item);
                await _context.SaveChangesAsync();               
            }
        }
        private HistoryItem CreateHistoryItem(string modelType, string modelJson, Guid historyModelEFId, HistoryAction action)
        {
            return new HistoryItem
            {
                Id = Guid.NewGuid(),
                ModelType = modelType,
                ModelJson = modelJson,
                HistoryModelEFId = historyModelEFId,
                Action = action
            };
        }
    }
}
