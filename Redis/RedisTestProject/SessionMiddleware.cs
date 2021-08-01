using Microsoft.AspNetCore.Http;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisTestProject
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RedisProvider _redisProvider;

        public SessionMiddleware(RequestDelegate next, RedisProvider redisProvider)
        {
            _next = next;
            _redisProvider = redisProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            // AuthenticationHandler 같은 걸 통해서 특정 URL은 auth 체크를 하지 않도록 하는 것도 방법이다
            if (context.Request.Path.StartsWithSegments("/login")) {
                await _next(context).ConfigureAwait(false);
                return;
            }

            var client = _redisProvider.GetClient();
            var dataBase = client.Database;

            var authToken = context.Request.Headers["Authorization"].FirstOrDefault();
            var key = $"User:{authToken}";
            var userData = await client.HashGetAllAsync<object>(key).ConfigureAwait(false);
            context.Items["User"] = new UserInfo(userData);

            await _next(context).ConfigureAwait(false);

            // end of request
            if (context.Items["User"] is UserInfo userInfo) {
                if (userInfo.UpdateData.Count > 0) {
                    var entries = userInfo.UpdateData.GetEntries(client.Serializer);
                    if (entries != null) {
                        var transaction = dataBase.CreateTransaction();
                        await transaction.HashSetAsync(key, entries, CommandFlags.FireAndForget).ConfigureAwait(false);
                        await transaction.KeyExpireAsync(key, TimeSpan.FromMinutes(5), CommandFlags.FireAndForget).ConfigureAwait(false);
                        await transaction.ExecuteAsync(CommandFlags.FireAndForget).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}