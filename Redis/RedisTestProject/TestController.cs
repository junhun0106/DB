using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Tasks;

namespace RedisTestProject
{
    [Route("[controller]")]
    public class TestController : Controller
    {
        private readonly RedisProvider _redisProvider;

        public TestController(RedisProvider redisProvider)
        {
            _redisProvider = redisProvider;
        }

        [HttpGet("redis/{key}/{value}")]
        public async Task<ActionResult> RedisTest(string key, string value)
        {
            var sb = new StringBuilder();

            var client = _redisProvider.GetClient();
            await client.Database.StringSetAsync(key, value);
            var get = await client.Database.StringGetAsync(key);
            sb.AppendLine($"added {key} : {get}");

            return Content(sb.ToString());
        }
    }
}
