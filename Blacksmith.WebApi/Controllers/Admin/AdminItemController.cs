using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models.Items;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;
using System.Text.RegularExpressions;

namespace Blacksmith.WebApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminItemController : ControllerBase
    {
        private readonly DbContextSqlite _db;
        private readonly ItemHelper _itemHelper;

        public AdminItemController(DbContextSqlite context, ItemHelper itemHelper)
        {
            _itemHelper = itemHelper;
            _db = context;
        }

        // Retrieves a paginated list of "Items" from the database as a JSON response.
        [HttpGet]
        public async Task<ActionResult<ItemManagerResponseDTO>> GetItems(int pageNumber, int pageSize, string? searchQuery, int lastId)
        {
            if (!ModelState.IsValid || pageSize == null || pageNumber == null || lastId == null)
                return BadRequest("Invalid Request");

            if (searchQuery != null && Regex.IsMatch(searchQuery, @"^[a-zA-Z0-9\s]+$") == false)
                return BadRequest("Invalid characters");

            // Check Values, Set Mini/Max
            searchQuery = searchQuery == null ? string.Empty : searchQuery;
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 5 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            List<Item> itemList = await _db.Items
                .Where(x => x.Name.Contains(searchQuery))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (itemList.Count == 0 || itemList == null)
                return NotFound();

            // Prepare Return Object 
            var result = new ItemManagerResponseDTO()
            {
                LastItemId = itemList.Max(x => x.Id),
                Data = new List<ItemDTO>()
            };

            foreach (Item item in itemList)
                result.Data.Add(_itemHelper.MapItemDTO(item));

            return Ok(result);
        }

        // Retrieves a single item by id as a JSON response.
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<ItemDTO>> GetItemById(int id)
        {
            if (!ModelState.IsValid || id == null)
                return BadRequest("Invalid Request");

            Item item = await _db.Items.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                return NotFound();

            return Ok(_itemHelper.MapItemDTO(item));
        }

        // Creates a new database entry
        [HttpPost("Create")]
        public async Task<ActionResult<ItemDTO>> CreateItem(ItemDTO itemDto)
        {
            if (!ModelState.IsValid || itemDto == null)
                return BadRequest();

            Item newItem = _itemHelper.MapItem(itemDto);

            // Set Values for potential nullables
            newItem.Name ??= new Item().Name;
            newItem.Description ??= new Item().Description;
            newItem.Image ??= new Item().Image;
            newItem.Recipe ??= new Item().Recipe;
            newItem.CraftId ??= new Item().CraftId;
            newItem.Prefix ??= new Item().Prefix;
            newItem.Suffix ??= new Item().Suffix;

            _db.Items.Add(newItem);
            await _db.SaveChangesAsync();

            return Ok(newItem);
        }

        // Deletes a single item entry by id.
        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> DeleteItemById(int id)
        {
            if (!ModelState.IsValid || id == null)
                return BadRequest();

            Item item = await _db.Items.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                return NotFound();

            _db.Items.Remove(item);
            await _db.SaveChangesAsync();

            return Ok();
        }

        // Updates a single item entry by id.
        [HttpPut("Update/{id}")]
        public async Task<ActionResult<ItemDTO>> UpdateItemById(ItemDTO itemDto)
        {
            if (!ModelState.IsValid || itemDto == null)
                return BadRequest();

            Item existingItem = await _db.Items.FindAsync(itemDto.Id);

            if (existingItem == null)
                return NotFound();

            // Map dto to item without a new object helper
            itemDto.Adapt(existingItem);

            // Set Values for potential nullables
            existingItem.Name ??= new Item().Name;
            existingItem.Description ??= new Item().Description;
            existingItem.Image ??= new Item().Image;
            existingItem.Recipe ??= new Item().Recipe;
            existingItem.CraftId ??= new Item().CraftId;
            existingItem.Prefix ??= new Item().Prefix;
            existingItem.Suffix ??= new Item().Suffix;

            await _db.SaveChangesAsync();

            return Ok(itemDto);
        }
    }
}
