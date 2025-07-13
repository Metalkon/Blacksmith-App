using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blacksmith.WebApi.Data;
using Shared_Classes.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Blacksmith.WebApi.Models.Items;

// --------------------------
// This is an outdated Item Controller from the old prototype project, it's been modified slightly but it will need to be overhauled.
// --------------------------

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
        
        // Retrieves a paginated list of "items" from the database as a JSON response.
        [HttpGet]
        public async Task<ActionResult<ItemManagerResponseDTO>> GetItems(int pageNumber, int pageSize, string? searchQuery, int lastId)
        {
            if (!ModelState.IsValid || pageSize == null || pageNumber == null || lastId == null)
            {
                return BadRequest("Invalid Request");
            }

            if (searchQuery != null && Regex.IsMatch(searchQuery, @"^[a-zA-Z0-9\s]+$") == false)
            {
                return BadRequest("Invalid characters");
            }

            // Check Values, Set Mini/Max
            searchQuery = searchQuery == null ? string.Empty : searchQuery;
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 5 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            List<Item>itemList = await _db.Items
                .Where(x => x.Name.Contains(searchQuery))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (itemList.Count == 0 || itemList == null)
            {
                return NotFound();
            }

            var result = new ItemManagerResponseDTO()
            {
                LastItemId = itemList.Max(item => item.Id),
                Data = new List<ItemEditDTO>()
            };

            foreach (Item item in itemList)
            {
                ItemEditDTO itemDto = await _itemHelper.MapItemToEditDTO(item);
                result.Data.Add(itemDto);
            }
            return Ok(result);
        }

        
        // Retrieves a single item by id as a JSON response.
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<ItemEditDTO>> GetItemById(int id)
        {
            if (!ModelState.IsValid || id == null)
            {
                return BadRequest("Invalid Request");
            }

            Item item = await _db.Items.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            ItemEditDTO itemDto = await _itemHelper.MapItemToEditDTO(item);

            return Ok(itemDto);
        }

        
        // Creates a new database entry
        [HttpPost]
        public async Task<ActionResult<ItemEditDTO>> CreateItem(ItemEditDTO itemDto)
        {
            if (!ModelState.IsValid || itemDto == null)
            {
                return BadRequest();
            }

            // Check Values, Set Defaults
            /*
            itemDto.Id = itemDto.Id == null ? 0 : itemDto.Id;
            itemDto.Name = string.IsNullOrEmpty(itemDto.Name) ? "Untitled" : itemDto.Name;
            itemDto.Type = string.IsNullOrEmpty(itemDto.Type) ? "None" : itemDto.Type;
            itemDto.Quantity = (itemDto.Quantity == null || itemDto.Quantity == 0) ? 1 : itemDto.Quantity;
            itemDto.Price = itemDto.Price == null ? 0 : itemDto.Price;
            itemDto.Tradable = itemDto.Tradable == null ? false : itemDto.Tradable;
            itemDto.Image = itemDto.Image == null ? "./images/Icon/Question_Mark.jpg" : itemDto.Image;
            */
            Item newItem = await _itemHelper.MapDTOToItem(itemDto);

            _db.Items.Add(newItem);
            await _db.SaveChangesAsync();
            return Ok(_itemHelper.MapItemToEditDTO(newItem));
        }

        // Deletes a single item entry by id.
        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult<ItemEditDTO>> DeleteItemById(int id)
        {
            if (!ModelState.IsValid || id == null)
            {
                return BadRequest();
            }

            Item item = await _db.Items.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound();
            }
            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
                
        // Updates a single item entry by id.
        [HttpPut]
        public async Task<ActionResult<ItemEditDTO>> UpdateItemById(ItemEditDTO itemDto)
        {
            if (!ModelState.IsValid || itemDto == null)
            {
                return BadRequest();
            }

            Item existingItem = await _db.Items.FindAsync(itemDto.Id);
            if (existingItem == null)
            {
                return NotFound();
            }
            if (existingItem != null)
            {
                existingItem = await _itemHelper.MapDTOToItem(itemDto, existingItem);

                // Check Values, Set Defaults
                /*
                existingItem.Id = existingItem.Id == null ? 0 : existingItem.Id;
                existingItem.Name = string.IsNullOrEmpty(existingItem.Name) ? "Untitled" : existingItem.Name;
                existingItem.Type = string.IsNullOrEmpty(existingItem.Type) ? "None" : existingItem.Type;
                existingItem.Quantity = (existingItem.Quantity == null || existingItem.Quantity == 0) ? 1 : existingItem.Quantity;
                existingItem.Price = existingItem.Price == null ? 0 : existingItem.Price;
                existingItem.Tradable = existingItem.Tradable == null ? false : existingItem.Tradable;
                existingItem.Image = existingItem.Image == null ? "./images/Icon/Question_Mark.jpg" : existingItem.Image;
                */

                await _db.SaveChangesAsync();
                return Ok(itemDto);
            }
            return BadRequest();
        }
        


    }
}
