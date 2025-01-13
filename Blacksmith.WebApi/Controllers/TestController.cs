using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
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

        [HttpGet("CombineItem")]
        public async Task<ActionResult<ItemEditDTO>> TestItem(int id)
        {
            if (!ModelState.IsValid) return BadRequest();

            GameData userData = await _db.GameData.FirstOrDefaultAsync(x => x.Id == 1);
            Item baseItem = await _db.Items.FirstOrDefaultAsync(x => x.Id == 1);
            List<ItemCrafted> userItems = userData.UserItems;
            ItemCrafted item = userItems[0];

            // NOTE: Add Unique ID (8 digits?) to crafted items, and figure out how to do the above more efficiently... any use 2 decimal doubles for some stats.

            ItemDTO testItem = new ItemDTO()
            {
                Name = baseItem.Name,
                Rarity = baseItem.Rarity,
                Tier = baseItem.Tier,
                Weight = baseItem.Weight,
                Description = baseItem.Description,
                Image = baseItem.Image,
                Recipe = baseItem.Recipe,
                Tradable = baseItem.Tradable,
                Durability = baseItem.BaseDurability + (int)(baseItem.BaseDurability * ((double)item.Score / 1000)),
                Price = item.Price,
                Score = item.Score,
                AttackPower = baseItem.BaseAttackPower + (int)(baseItem.BaseAttackPower * ((double)item.Score / 1000)),
                AttackSpeed = baseItem.BaseAttackSpeed + (int)(baseItem.BaseAttackSpeed * ((double)item.Score / 1000)),
                MagicPower = baseItem.BaseMagicPower + (int)(baseItem.BaseMagicPower  * ((double)item.Score / 1000)),
                ProtectionPhysical = baseItem.BaseProtectionPhysical + (int)(baseItem.BaseProtectionPhysical * ((double)item.Score / 1000)),
                ProtectionMagic = baseItem.BaseProtectionMagic + (int)(baseItem.BaseProtectionMagic * ((double)item.Score / 1000)),
                Prefix = item.PrefixId.ToString(),
                Suffix = item.SuffixId.ToString()
            };

            //_db.Items.Add(testItem);
            //_db.SaveChanges();

            return Ok(testItem);
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
