using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared_Classes.Models;

namespace Blacksmith.WebApi.Controllers
{
    [Route("api/Test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost("TestClass")]
        public async Task<ActionResult<TestClass>> TestClass(TestClass test)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            TestClass response = new TestClass()
            {
                TestProperty = $"Hello World, {test.TestProperty}"
            };                
            return Ok(response);
        }
    }
}
