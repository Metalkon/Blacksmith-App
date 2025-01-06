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
    [Authorize(Roles = "Admin")]
    public class AdminItemController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AdminItemController(ApplicationDbContext context)
        {
            _db = context;
        }
        /*
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

            List<Recipe>itemList = await _db.Recipes
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
                LastId = itemList.Max(item => item.Id),
                Data = new List<ItemDTO>()
            };

            foreach (Item item in itemList)
            {
                ItemDTO itemDto = await MapItemToDTO(item);
                result.Data.Add(itemDto);
            }
            return Ok(result);
        }

        
        // Retrieves a single item by id as a JSON response.
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<ItemDTO>> GetItemById(int id)
        {
            if (!ModelState.IsValid || id == null)
            {
                return BadRequest("Invalid Request");
            }

            Item item = await _db.Recipes.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            ItemDTO itemDto = await MapItemToDTO(item);

            return Ok(itemDto);
        }

        // Creates a new database entry
        [HttpPost]
        public async Task<ActionResult<ItemDTO>> CreateItem(ItemDTO itemDto)
        {
            if (!ModelState.IsValid || itemDto == null)
            {
                return BadRequest();
            }

            // Check Values, Set Defaults
            itemDto.Id = itemDto.Id == null ? 0 : itemDto.Id;
            itemDto.Name = string.IsNullOrEmpty(itemDto.Name) ? "Untitled" : itemDto.Name;
            itemDto.Type = string.IsNullOrEmpty(itemDto.Type) ? "None" : itemDto.Type;
            itemDto.Quantity = (itemDto.Quantity == null || itemDto.Quantity == 0) ? 1 : itemDto.Quantity;
            itemDto.Price = itemDto.Price == null ? 0 : itemDto.Price;
            itemDto.Tradable = itemDto.Tradable == null ? false : itemDto.Tradable;
            itemDto.Image = itemDto.Image == null ? "./images/Icon/Question_Mark.jpg" : itemDto.Image;

            Item newItem = await MapDTOToItem(itemDto);

            _db.Recipes.Add(newItem);
            await _db.SaveChangesAsync();

            return Ok(MapItemToDTO(newItem));
        }

        // Deletes a single item entry by id.
        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult<ItemDTO>> DeleteItemById(int id)
        {
            if (!ModelState.IsValid || id == null)
            {
                return BadRequest();
            }

            Item item = await _db.Recipes.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound();
            }
            _db.Recipes.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Updates a single item entry by id.
        [HttpPut]
        public async Task<ActionResult<ItemDTO>> UpdateItemById(ItemDTO itemDto)
        {
            if (!ModelState.IsValid || itemDto == null)
            {
                return BadRequest();
            }

            Item existingItem = await _db.Recipes.FindAsync(itemDto.Id);
            if (existingItem == null)
            {
                return NotFound();
            }
            if (existingItem != null)
            {
                existingItem = await MapDTOToItem(itemDto, existingItem);

                // Check Values, Set Defaults
                existingItem.Id = existingItem.Id == null ? 0 : existingItem.Id;
                existingItem.Name = string.IsNullOrEmpty(existingItem.Name) ? "Untitled" : existingItem.Name;
                existingItem.Type = string.IsNullOrEmpty(existingItem.Type) ? "None" : existingItem.Type;
                existingItem.Quantity = (existingItem.Quantity == null || existingItem.Quantity == 0) ? 1 : existingItem.Quantity;
                existingItem.Price = existingItem.Price == null ? 0 : existingItem.Price;
                existingItem.Tradable = existingItem.Tradable == null ? false : existingItem.Tradable;
                existingItem.Image = existingItem.Image == null ? "./images/Icon/Question_Mark.jpg" : existingItem.Image;

                await _db.SaveChangesAsync();
                return Ok(itemDto);
            }
            return BadRequest();
        }
        */

        private async Task<ItemDTO> MapItemToDTO(Item input)
        {
            ItemDTO itemDTO = new ItemDTO();

            var inputProperties = typeof(Item).GetProperties();
            var itemDTOProperties = typeof(ItemDTO).GetProperties();

            foreach (var inputProp in inputProperties)
            {
                var itemDTOProp = itemDTOProperties.FirstOrDefault(p => p.Name == inputProp.Name && p.PropertyType == inputProp.PropertyType);

                if (itemDTOProp != null && itemDTOProp.CanWrite)
                {
                    var value = inputProp.GetValue(input);
                    itemDTOProp.SetValue(itemDTO, value);
                }
            }

            return itemDTO;
        }

        private async Task<Item> MapDTOToItem(ItemDTO input)
        {
            Item item = new Item();

            var inputProperties = typeof(ItemDTO).GetProperties();
            var itemProperties = typeof(Item).GetProperties();

            foreach (var inputProp in inputProperties)
            {
                var itemProp = itemProperties.FirstOrDefault(p => p.Name == inputProp.Name && p.PropertyType == inputProp.PropertyType);

                if (itemProp != null && itemProp.CanWrite)
                {
                    var value = inputProp.GetValue(input);
                    itemProp.SetValue(item, value);
                }
            }

            return item;
        }

        private async Task<Item> MapDTOToItem(ItemDTO input, Item itemDb)
        {
            var inputProperties = typeof(ItemDTO).GetProperties();
            var itemProperties = typeof(Item).GetProperties();

            foreach (var inputProp in inputProperties)
            {
                var itemProp = itemProperties.FirstOrDefault(p => p.Name == inputProp.Name && p.PropertyType == inputProp.PropertyType);

                if (itemProp != null && itemProp.CanWrite)
                {
                    var value = inputProp.GetValue(input);
                    itemProp.SetValue(itemDb, value);
                }
            }

            return itemDb;
        }
    }
}
