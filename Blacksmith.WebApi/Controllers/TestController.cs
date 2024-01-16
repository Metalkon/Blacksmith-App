using Blacksmith.WebApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;

namespace Blacksmith.WebApi.Controllers
{
    [Route("api/Test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public TestController(ApplicationDbContext context)
        {
            _db = context;
        }

        [HttpGet("PotatoName")]
        public async Task<ActionResult<TestPotato>> PotatoName(string input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            TestPotato findPotato = await _db.TestPotatoes.FirstOrDefaultAsync(x => x.Name.Contains(input));
            if (findPotato == null)
            {
                return NotFound();
            }             
            return Ok(findPotato);
        }
    }
}
