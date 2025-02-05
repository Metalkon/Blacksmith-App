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
    [Authorize(Roles = "Admin")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ItemManager _itemManager;

        public TestController(ApplicationDbContext context, ItemManager itemManager)
        {
            _db = context;
            _itemManager = itemManager;
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


/*
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

        [HttpGet("ResetInventory")]
        public async Task<ActionResult<TestPotato>> ResetInventory()
        {
            if (!ModelState.IsValid) return BadRequest();

            GameData userData = await _db.GameData.FirstOrDefaultAsync(x => x.Id == 1);
            List<ItemCrafted> userItems = userData.UserItems;
            userItems.Clear();
            userItems.Add(new ItemCrafted());
            _db.SaveChanges();

            return Ok();
        }

        [HttpGet("CombineItem")]
        public async Task<ActionResult<ItemDTO>> CombineItem(int userId, string craftId)
        {
            if (!ModelState.IsValid) return BadRequest();

            List<ItemCrafted> userItems = (await _db.GameData.FirstAsync(x => x.Id == userId)).UserItems;
            ItemCrafted item = userItems.FirstOrDefault(x => x.CraftId == craftId);

            ItemDTO testItem = await _itemManager.GenerateItemDTO(item);

            //_db.Items.Add(testItem);
            //_db.SaveChanges();

            return Ok(testItem);
        }

        [HttpGet("EditItem")]
        public async Task<ActionResult<ItemEditDTO>> AddItem(int userId, string craftId, int newScore)
        {
            if (!ModelState.IsValid) return BadRequest();

            List<ItemCrafted> userItems = (await _db.GameData.FirstAsync(x => x.Id == userId)).UserItems;
            ItemCrafted item = userItems.FirstOrDefault(x => x.CraftId == craftId);
            item.Score = newScore;

            _db.SaveChanges();

            return Ok();
        }

        [HttpGet("NewItem")]
        public async Task<ActionResult<ItemEditDTO>> NewItem(int userId, string craftId, int newScore, int itemId)
        {
            if (!ModelState.IsValid) return BadRequest();

            List<ItemCrafted> userItems = (await _db.GameData.FirstAsync(x => x.Id == userId)).UserItems;
            userItems.Add(new ItemCrafted()
            {
                ItemId = itemId,
                Score = newScore
            });

            _db.SaveChanges();

            return Ok();
        }
 */ 