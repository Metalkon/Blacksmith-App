using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models.Items;
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

        [HttpGet("TestItem")]
        public async Task<ActionResult<TestPotato>> TestItem(string name)
        {
            if (!ModelState.IsValid) return BadRequest();

            Item testItem = new Item()
            {
                Name = name,
                Tier = 1,
                Weight = 5.0,
                Description = "Test item description"
            };
            _db.Items.Add(testItem);
            _db.SaveChanges();

            return Ok();
        }

        [HttpGet("PotatoName")]
        public async Task<ActionResult<TestPotato>> PotatoName(string input)
        {
            if (!ModelState.IsValid) return BadRequest();


            TestPotato findPotato = await _db.TestPotatoes.FirstOrDefaultAsync(x => x.Name.Contains(input));
            if (findPotato == null)
            {
                return NotFound();
            }

            return Ok(findPotato);
        }

    }
}
