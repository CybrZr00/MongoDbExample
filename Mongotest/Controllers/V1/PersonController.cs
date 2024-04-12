using Blueshift.EntityFrameworkCore.MongoDB;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mongotest.Data;
using Mongotest.Models.V1;

using OpenIddict.Validation.AspNetCore;

namespace Mongotest.Controllers.V1
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Route("api/V1/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly IApplicationDA _db;
        private readonly ILogger<PersonController> _logger;
        private readonly ApplicationEFContext _context;

        public PersonController(IApplicationDA db, ILogger<PersonController> logger, ApplicationEFContext context)
        {
            _db = db; 
            _logger = logger;
            _context = context;
        }
        // GET: api/<PersonController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonModel>>> Get([FromQuery] bool useEF)
        {
            try
            {
                if (useEF)
                {
                    return Ok(await _context.People.ToListAsync());
                }
                return Ok(await _db.GetAllAsync<PersonModel>(nameof(PersonModel)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all people");
                return BadRequest(ex);
            }
        }

        // GET api/<PersonController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PersonModel?>> Get(string id, [FromQuery] bool useEF)
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
                return BadRequest(ex);
            }
        }
        // POST api/<PersonController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PersonModel person, [FromQuery] bool useEF)
        {
            try
            {
                if (useEF)
                {
                    await _context.People.AddAsync(person);
                    await _context.SaveChangesAsync();
                    return Created();
                }
                //person.Id = Guid.NewGuid().ToString();
                await _db.CreateAsync(person, nameof(PersonModel));
                return Created();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PersonModel record");
                return BadRequest(ex);
            }
        }
        // PUT api/<PersonController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, [FromBody] PersonModel person, [FromQuery] bool useEF)
        {
            try
            {
                if(id != person.Id)
                {
                    return BadRequest("Ambiguous identity");
                }
                if (useEF)
                {
                    var personToBeUpdated = await _context.People.FindAsync(id);
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
                return BadRequest(ex);
            }
        }
        // DELETE api/<PersonController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id, [FromQuery] bool useEF)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest("Invalid");
                }
                if (useEF)
                {
                    var personToBeDeleted = await _context.People.FindAsync(id);
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
                return BadRequest(ex);
            }
        }
    }
}
