using Microsoft.AspNetCore.Mvc;

using Mongotest.Data;
using Mongotest.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Mongotest.Controllers.V1
{
    [Route("api/V1/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly IApplicationDA _db;

        public PersonController(IApplicationDA db)
        {
            _db = db; 
        }
        // GET: api/<PersonController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonModel>>> Get()
        {
            try
            {
                return Ok(await _db.GetAllAsync<PersonModel>(nameof(PersonModel)));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // GET api/<PersonController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PersonModel?>> Get(string id)
        {
            try
            {
                return Ok(await _db.GetOneAsync<PersonModel>(id, nameof(PersonModel)));
            }
            catch (Exception ex)
            {

                return BadRequest(ex);
            }
        }

        // POST api/<PersonController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PersonModel person)
        {
            try
            {
                //person.Id = Guid.NewGuid().ToString();
                await _db.CreateAsync(person, nameof(PersonModel));
                return Created();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        // PUT api/<PersonController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, [FromBody] PersonModel person)
        {
            try
            {
                await _db.UpsertAsync(id, person, nameof(PersonModel));
                return Accepted();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        // DELETE api/<PersonController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                await _db.DeleteAsync<PersonModel>(id, nameof(PersonModel));
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
