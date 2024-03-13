using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
using Blacksmith.WebApi.Services;
using Shared_Classes.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using static System.Net.Mime.MediaTypeNames;

// --------------------------
// This is an outdated Item Controller from the old prototype project, it's been modified slightly but it will need to be overhauled.
// --------------------------

namespace Blacksmith.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminItemController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AdminItemController(ApplicationDbContext context)
        {
            _db = context;
        }

        // Retrieves a paginated list of "items" from the database as a JSON response.
        [HttpGet]
        public async Task<ActionResult<ItemManagerResponseDTO>> GetItems(int pageNumber, int pageSize, string? searchQuery, int lastItemId)
        {
            if (pageNumber < 0 || pageSize < 0 || lastItemId < 0)
            {
                return BadRequest();
            }
            if (searchQuery != null && !Regex.IsMatch(searchQuery, @"^[a-zA-Z0-9\s]+$"))
            {
                return BadRequest();
            }

            // Check Values, Set Minimums
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            IQueryable<Item> queryItem = _db.Items.AsQueryable();

            // Search Query
            if (searchQuery != null)
            {
                queryItem = queryItem.Where(item => item.Name.Contains(searchQuery));
            }

            // Pagination
            queryItem = queryItem.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            List<Item> items = await queryItem.ToListAsync();
            if (items.Count == 0)
            {
                return NotFound();
            }
            var result = new ItemManagerResponseDTO
            {
                LastItemId = items.Max(item => item.Id),
                Data = items.Select(item => new ItemDTO
                {
                    Id = item.Id,
                    ItemId = item.ItemId,
                    Type = item.Type,
                    Name = item.Name,
                    Price = item.Price,
                    Tradable = item.Tradable,
                    Image = item.Image
                }).ToList()
            };
            return Ok(result);
        }

        // Retrieves a single item by id as a JSON response.
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<ItemDTO>> GetItemById(int id)
        {
            Item itemGet = await _db.Items.FirstOrDefaultAsync(x => x.Id == id);
            if (itemGet == null)
            {
                return NotFound();
            }
            var result = new ItemDTO()
            {
                Id = itemGet.Id,
                ItemId = itemGet.ItemId,
                Type = itemGet.Type,
                Name = itemGet.Name,
                Price = itemGet.Price,
                Tradable = itemGet.Tradable,
                Image = itemGet.Image
            };
            return Ok(result);
        }

        // Creates a new database entry
        [HttpPost]
        public async Task<ActionResult<ItemDTO>> CreateItem(ItemDTO item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Check Values, Set Defaults
            item.Name = item.Name == null ? "Untitled" : item.Name;
            item.Description = item.Description == null ? "N/A" : item.Description;
            item.Price = item.Price == null ? 0 : item.Price;
            item.Rarity = item.Rarity == null ? "Common" : item.Rarity;
            item.Image = item.Image == null ? "./images/Icon/Question_Mark.jpg" : item.Image;

            var newItem = new Item()
            {
                Id = item.Id,
                ItemId = item.ItemId,
                Type = item.Type,
                Name = item.Name,
                Price = item.Price,
                Tradable = item.Tradable,
                Image = item.Image
            };
            _db.Items.Add(newItem);
            await _db.SaveChangesAsync();
            var newItemDTO = new ItemDTO()
            {
                Id = newItem.Id,
                Name = newItem.Name
            };
            return CreatedAtAction(nameof(GetItemById), new { id = newItemDTO.Id }, newItemDTO);
        }

        // Deletes a single item entry by id.
        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult<ItemDTO>> DeleteItemById(int id)
        {
            Item result = await _db.Items.FirstOrDefaultAsync(x => x.Id == id);
            if (result == null)
            {
                return NotFound();
            }
            _db.Items.Remove(result);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Updates a single item entry by id.
        [HttpPut]
        public async Task<ActionResult<ItemDTO>> UpdateItemById(ItemDTO item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            Item existingItem = await _db.Items.FindAsync(item.Id);
            if (existingItem == null)
            {
                return NotFound();
            }

            // Check Values, Set Defaults
            item.Name = item.Name == null ? "Untitled" : item.Name;
            item.Description = item.Description == null ? "N/A" : item.Description;
            item.Price = item.Price == null ? 0 : item.Price;
            item.Rarity = item.Rarity == null ? "Common" : item.Rarity;
            item.Image = item.Image == null ? "./images/Icon/Question_Mark.jpg" : item.Image;

            {
                existingItem.Id = item.Id;
                existingItem.ItemId = item.ItemId;
                existingItem.Type = item.Type;
                existingItem.Name = item.Name;
                existingItem.Price = item.Price;
                existingItem.Tradable = item.Tradable;
                existingItem.Image = item.Image;
            }
            await _db.SaveChangesAsync();
            var updatedItem = new ItemDTO()
            {
                Id = existingItem.Id,
                Name = existingItem.Name
            };
            return Ok(updatedItem);
        }
    }
}
