using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
using Blacksmith.WebApi.Models.Items;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;

namespace Blacksmith.WebApi.Controllers
{
    [Route("api/Test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly DbContextSqlServer _db;
        private readonly ItemHelper _itemHelper;

        public TestController(DbContextSqlServer context, ItemHelper itemHelper)
        {
            _db = context;
            _itemHelper = itemHelper;
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

        [HttpGet("ReturnItemHelper")]
        public async Task<ActionResult<ItemDTO>> ReturnItemHelper()
        {
            if (!ModelState.IsValid) return BadRequest();

            ItemCrafted test = new ItemCrafted()
            {
                ItemId = 2,
                CraftId = new Guid().ToString(),
                PrefixId = 1,
                SuffixId = 1,
                Score = 83,
                Durability = 55,
                Price = 500,
            };

            var test2 = await _itemHelper.MapCraftedItemToItemDTO(test);

            return Ok(test2);
        }

    }
}