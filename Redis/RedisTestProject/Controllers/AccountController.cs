using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace RedisTestProject.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly RedisProvider _redisProvider;

        public AccountController(RedisProvider redisProvider)
        {
            _redisProvider = redisProvider;
        }

        // 레디스를 이용하여 토큰 관리
        [HttpGet("login")]
        public async Task<ActionResult> AccountLogin()
        {
            var guid = Guid.NewGuid().ToString();

            var client = _redisProvider.GetClient();
            var dataBase = client.Database;
            var key = $"User:{guid}";
            var exists = await dataBase.KeyExistsAsync(key).ConfigureAwait(false);
            if (exists) {
                // error - 서버에서 발급하는 guid가 겹침. 재시도 할 수 있도록 처리 해야 함
            }
            var userInfo = new UserInfo {
                Token = key,
                Value = 1,
            };
            var entries = userInfo.UpdateData.GetEntries(client.Serializer);
            if (entries != null) {
                var transaction = dataBase.CreateTransaction();
                await transaction.HashSetAsync(key, entries, CommandFlags.FireAndForget).ConfigureAwait(false);
                await transaction.KeyExpireAsync(key, TimeSpan.FromMinutes(5), CommandFlags.FireAndForget).ConfigureAwait(false);
                await transaction.ExecuteAsync(CommandFlags.FireAndForget).ConfigureAwait(false);
            }
            return Json(guid);
        }

        [HttpGet("levelup")]
        public async Task<ActionResult> AccountLevelUp()
        {
            var user = (UserInfo)HttpContext.Items["User"];
            user.Value++;

            return Ok("Ok");
        }
    }
}
