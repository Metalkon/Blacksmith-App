using Blacksmith.WebApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blacksmith.WebApi.Controllers.Items
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class InventoryController : ControllerBase
    {
        private readonly DbContextSqlServer _db;
        private readonly ItemManager _itemManager;

        public InventoryController(DbContextSqlServer context, ItemManager itemManager)
        {
            _db = context;
            _itemManager = itemManager;
        }

    }
}
