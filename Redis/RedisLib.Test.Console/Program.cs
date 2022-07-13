using System.Diagnostics;
using System.Text;
using StackExchange.Redis;
using RedisLib;

Process redisProcess = null;

// https://github.com/microsoftarchive/redis
var processes = Process.GetProcessesByName("redis-server");
if (processes == null || processes.Length == 0)
{
    var fullPath = Path.GetFullPath("Executable/redis-server.exe");
    redisProcess = Process.Start(fullPath);
}
else
{
    redisProcess = processes[0];
}

Task.Run(async () =>
{
    var provider = new RedisProvider("localhost:6379,allowAdmin=true");
    var db = provider.Get();
    try
    {
        Console.WriteLine("FlushDbAsync start");
        await db.FlushDbAsync();
        Console.WriteLine("FlushDbAsync end");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FlushDbAsync - {ex.Message}");
    }
    var ignore = new List<string>
    {
        "get_Database",
        "get_Serializer",
        "Equals",
        "ToString",
        "GetHashCode",
        "GetType",
        "FlushDbAsync",
        "HashScanAsync",
        "SearchKeysAsync",
        "SaveAsync",
        "PublishAsync",
        "SubscribeAsync",
        "UnsubscribeAsync",
        "UnsubscribeAllAsync",
    };

    Console.WriteLine($"test ignore func : {string.Join(",", ignore)}");
    Console.WriteLine();

    var type = db.GetType();
    var methods = type.GetMethods();
    // Methods
    foreach (var method in methods)
    {
        var sb = new StringBuilder();

        if (ignore.Contains(method.Name))
        {
            continue;
        }

        sb.Append(method.Name).Append('(');
        var parameters = method.GetParameters();
        var obj = new List<object>(parameters.Length);
        for (int i = 0; i < parameters.Length; ++i)
        {
            var parameter = parameters[i];
            if (i == parameters.Length - 1)
            {
                sb.Append($"{parameter.ParameterType} {parameter.Name}");
            }
            else
            {
                sb.Append($"{parameter.ParameterType} {parameter.Name}, ");
            }

            if (parameter.Name == "key" || parameter.Name == "hashKey")
            {
                obj.Add(method.Name);
            }
            else if (parameter.Name == "keys")
            {
                obj.Add(new string[] { method.Name });
            }
            else if (parameter.Name == "items")
            {
                obj.Add(new ValueTuple<string, string>[] { (method.Name, "bar") });
            }
            else if (parameter.Name == "value")
            {
                if (parameter.ParameterType == typeof(int))
                {
                    obj.Add(1);
                }
                else if (parameter.ParameterType == typeof(long))
                {
                    obj.Add(1);
                }
                else if (parameter.ParameterType == typeof(double))
                {
                    obj.Add(1D);
                }
                else
                {
                    obj.Add("bar");
                }
            }
            else if (parameter.Name == "values")
            {
                obj.Add(new string[] { "bar", "bar1", "bar2" });
            }
            else if (parameter.Name == "hashEntries")
            {
                obj.Add(new Dictionary<string, string> { { "foo", "bar" }, { "foo1", "bar1" } });
            }
            else if (parameter.Name == "expiresAt")
            {
                obj.Add((DateTimeOffset)DateTime.Now.AddMinutes(5));
            }
            else if (parameter.Name == "expiresIn")
            {
                obj.Add(TimeSpan.FromMinutes(5));
            }
            else if (parameter.Name == "flag")
            {
                obj.Add(CommandFlags.FireAndForget);
            }
            else if (parameter.Name == "notExists")
            {
                obj.Add(false);
            }
            else if (parameter.ParameterType == typeof(int))
            {
                obj.Add(1);
            }
            else if (parameter.ParameterType == typeof(long))
            {
                obj.Add(1);
            }
            else if (parameter.ParameterType == typeof(double))
            {
                obj.Add(1D);
            }
            else if (parameter.ParameterType == typeof(Exclude))
            {
                obj.Add(Exclude.None);
            }
            else if (parameter.ParameterType == typeof(Order))
            {
                obj.Add(Order.Ascending);
            }
            else if (parameter.Name == "when")
            {
                obj.Add(When.Always);
            }
            else
            {
                Console.WriteLine($"[Warn] define parameter : {method.Name}(..{parameter}..)");
            }
        }
        sb.Append(')');

        var name = $"{method.Name}({string.Join(",", obj)})";
        Console.WriteLine($"{name} start");

        try
        {
            if (method.Name == "HashIncerementByAsync")
            {
                Console.WriteLine("");
            }

            if (method.IsGenericMethod)
            {
                await (dynamic)method.MakeGenericMethod(typeof(string)).Invoke(db, obj.ToArray());
            }
            else
            {
                await (dynamic)method.Invoke(db, obj.ToArray());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] {method.Name} : {ex.Message}");
        }

        Console.WriteLine($"{name} end");

        //Console.WriteLine(sb.ToString());
    }

    // SearchKeyAsync
    {
        Console.WriteLine();
        Console.WriteLine("SearchKeysAsync start");
        async Task SearchKeyAsyncTest()
        {
            const int count = 10;
            var addKeys = new List<string>(count);
            for (int i = 0; i < count; ++i)
            {
                var key = $"SearchKeysAsync:{i}";
                await db.AddAsync(key, "bar");
                addKeys.Add(key);
            }
            Console.WriteLine($"AddAsync(key: {string.Join(", ", addKeys)})");

            var keys = (await db.SearchKeysAsync("SearchKeysAsync:*")).ToList();
            if (keys.Count != 10)
            {
                Console.WriteLine($"[Error] SearchKeysAsync dismatch count. expect : {count}, search : {keys.Count}");
                return;
            }

            foreach (var key in addKeys)
            {
                if (!keys.Contains(key))
                {
                    Console.WriteLine($"[Error] SearchKeysAsync not found key : {key}");
                    return;
                }
            }

            Console.WriteLine("SearchKeysAsync success");
        }

        await SearchKeyAsyncTest();
        Console.WriteLine("SearchKeysAsync end");
    }

    // HashScanAsync
    {
        Console.WriteLine();
        async Task HashScanTest()
        {
            const int count = 10;
            var addKeys = new List<string>(count);
            for (int i = 0; i < count; ++i)
            {
                var key = $"HashScanAsync:{i}";
                await db.HashSetAsync("HashScanAsync", key, "bar");
                addKeys.Add(key);
            }
            Console.WriteLine($"HashSetAsync(key : {string.Join(",", addKeys)})");

            var dic = await db.HashScanAsync<string>("HashScan", "HashScan:*");

            if (dic.Count != 10)
            {
                Console.WriteLine($"[Error] SearchKeysAsync dismatch count. expect : {count}, search : {dic.Count}");
                return;
            }

            foreach (var key in addKeys)
            {
                if (!dic.ContainsKey(key))
                {
                    Console.WriteLine($"[Error] HashSetAsync not found key : {key}");
                    return;
                }
            }

            Console.WriteLine("HashScanAsync success");
        }
        Console.WriteLine("HashScanAsync start");
        await HashScanTest();
        Console.WriteLine("HashScanAsync end");
    }

    // Pub/Sub
    {
        Console.WriteLine();
        async Task PubSubTest()
        {
            var subDb = provider.Get(1);

            Console.WriteLine("Pub/Sub Subscribe start");
            await subDb.SubscribeAsync<string>("pubsub", message =>
            {
                Console.WriteLine($"Pub/Sub subscribe - {message}");

                return Task.CompletedTask;
            });

            var pubDb = provider.Get(1);

            Console.WriteLine("Pub/Sub publish - pubsub!");
            await pubDb.PublishAsync("pubsub", "pubsub!");

            Console.WriteLine("Pub/Sub subcribe - UnsubscribeAll");
            await subDb.UnsubscribeAllAsync();

            Console.WriteLine("Pub/Sub publish - pubsub2!");
            await pubDb.PublishAsync("pubsub", "pubsub2!");
        }
        Console.WriteLine("Pub/Sub start");
        await PubSubTest();
        Console.WriteLine("Pub/Sub end");
    }
});

Console.WriteLine("if you want exit, press 'e' or 'E'");
while (true)
{
    var line = Console.ReadLine();
    if (line.Contains('e', StringComparison.OrdinalIgnoreCase))
    {
        break;
    }
}

redisProcess?.Kill();
