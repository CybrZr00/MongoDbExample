using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MongoDB.Driver;

using Mongotest.Data;
using Shared.Models ;

using OpenIddict.Validation.AspNetCore;

using System.Collections;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Mongotest.Controllers.V1
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly IApplicationDA _db;
        private readonly ILogger<HistoryController> _logger;
        private readonly ApplicationEFContext _context;
        public HistoryController(IApplicationDA db, ILogger<HistoryController> logger)
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            _context = ApplicationEFContext.Create(mongoClient.GetDatabase("mongotest_v1_ef"));
            _db = db;
            _logger = logger;
        }
        // GET: api/<HistoryController>
        [HttpGet]
        public async Task<IActionResult> Get(bool useEF)
        {
            if (useEF)
            {
                var histories = await _context.Histories.ToArrayAsync();
                foreach (var history in histories)
                {
                    history.HistoryEntries = await _context.HistoryItems.Where(x => x.HistoryModelEFId == history.Id).ToListAsync();
                }
                return Ok(histories);
            }else
            {
                var histories = await _db.GetAllAsync<HistoryModel<PersonModel>>("HistoryModel<PersonModel>");
                return Ok(histories);
            }
        }

        // GET api/<HistoryController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, bool useEF)
        {
            try
            {
                if (useEF)
                {
                    var history = await _context.Histories.FirstOrDefaultAsync(x => x.ModelId == id);
                    if(history is null)
                    {
                        return NotFound();
                    }
                    history.HistoryEntries = await _context.HistoryItems.Where(x => x.HistoryModelEFId == history.Id).ToListAsync();
                    return Ok(history);
                }
                else
                {
                    var history = await _db.GetOneAsync<HistoryModel<PersonModel>>(id.ToString(), "HistoryModel<PersonModel>");
                    return Ok(history);
                }
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

    }
}
