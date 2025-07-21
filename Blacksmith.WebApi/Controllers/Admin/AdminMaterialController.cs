using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blacksmith.WebApi.Data;
using Shared_Classes.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Blacksmith.WebApi.Models.Items;
using Mapster;

// --------------------------
// This is an outdated Item Controller from the old prototype project, it's been modified slightly but it will need to be overhauled.
// --------------------------

namespace Blacksmith.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminMaterialController : ControllerBase
    {
        private readonly DbContextSqlite _db;
        private readonly ItemHelper _itemHelper;

        public AdminMaterialController(DbContextSqlite context, ItemHelper itemHelper)
        {
            _itemHelper = itemHelper;
            _db = context;
        }

        // Retrieves a paginated list of "Materials" from the database as a JSON response.
        [HttpGet]
        public async Task<ActionResult<MaterialManagerResponseDTO>> GetMaterials(int pageNumber, int pageSize, string? searchQuery, int lastId)
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

            List<Material> materialList = await _db.Materials
                .Where(x => x.Name.Contains(searchQuery))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (materialList.Count == 0 || materialList == null)
                return NotFound();

            // Prepare Return Object 
            var result = new MaterialManagerResponseDTO()
            {
                LastItemId = materialList.Max(x => x.Id),
                Data = new List<MaterialDTO>()
            };

            foreach (Material material in materialList)
                result.Data.Add(_itemHelper.MapMaterialDTO(material));

            return Ok(result);
        }

        // Retrieves a single material by id as a JSON response.
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<MaterialDTO>> GetMaterialById(int id)
        {
            if (!ModelState.IsValid || id == null)
                return BadRequest("Invalid Request");

            Material material = await _db.Materials.FirstOrDefaultAsync(x => x.Id == id);

            if (material == null)
                return NotFound();

            return Ok(_itemHelper.MapMaterialDTO(material));
        }

        // Creates a new database entry
        [HttpPost("Create")]
        public async Task<ActionResult<MaterialDTO>> CreateMaterial(MaterialDTO materialDto)
        {
            if (!ModelState.IsValid || materialDto == null)
                return BadRequest();

            Material newMaterial = _itemHelper.MapMaterial(materialDto);

            // Set Values for potential nullables
            newMaterial.Name ??= new Material().Name;
            newMaterial.Description ??= new Material().Description;
            newMaterial.Image ??= new Material().Image;

            _db.Materials.Add(newMaterial);
            await _db.SaveChangesAsync();

            return Ok(newMaterial);
        }

        // Deletes a single material entry by id.
        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> DeleteMaterialById(int id)
        {
            if (!ModelState.IsValid || id == null)
                return BadRequest();

            Material material = await _db.Materials.FirstOrDefaultAsync(x => x.Id == id);

            if (material == null)
                return NotFound();

            _db.Materials.Remove(material);
            await _db.SaveChangesAsync();

            return Ok();
        }

        // Updates a single material entry by id.
        [HttpPut("Update/{id}")]
        public async Task<ActionResult<MaterialDTO>> UpdateMaterialById(MaterialDTO materialDto)
        {
            if (!ModelState.IsValid || materialDto == null)
                return BadRequest();

            Material existingMaterial = await _db.Materials.FindAsync(materialDto.Id);

            if (existingMaterial == null)
                return NotFound();

            // Map dto to material without a new object helper
            materialDto.Adapt(existingMaterial);

            // Set Values for potential nullables
            existingMaterial.Name ??= new Material().Name;
            existingMaterial.Description ??= new Material().Description;
            existingMaterial.Image ??= new Material().Image;

            await _db.SaveChangesAsync();
            
            return Ok(materialDto);            
        }
    }
}