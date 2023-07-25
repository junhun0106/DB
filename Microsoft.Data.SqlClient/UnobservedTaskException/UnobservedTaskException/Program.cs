internal class Program
{
    class Context
    {
        public readonly TaskCompletionSource source;

        public Context(TaskCompletionSource source)
        {
            this.source = source;
        }
    }

    static async Task ExecuteWithFailover(Func<Task> func)
    {
        try
        {
            await func();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ExecuteWithFailover - {ex.Message}");
        }
        finally
        {
            Console.WriteLine("complete");
        }
    }

    static async Task ExecuteImplAsync()
    {
        try
        {
            await InternalExecuteNonQueryAsync();
        }
        finally
        {
        }
    }

    static void BeginExecuteNonQueryInternalReadStage(TaskCompletionSource<object> completion)
    {
        completion.SetException(new Exception("somting error"));

        // like _connection.Error(...) thrown
        throw new Exception("somting error");
    }

    static IAsyncResult BeginExecuteNonQueryInternal(AsyncCallback callback)
    {
        TaskCompletionSource<object> localCompletion = new TaskCompletionSource<object>();
        TaskCompletionSource<object> globalCompletion = new TaskCompletionSource<object>();

        try
        {
            BeginExecuteNonQueryInternalReadStage(localCompletion);
        }
        catch
        {
            throw;
        }

        localCompletion.Task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                globalCompletion.TrySetException(t.Exception.InnerException);
            }
            else if (t.IsCanceled)
            {
                globalCompletion.TrySetCanceled();
            }
            else
            {
                globalCompletion.TrySetResult(t.Result);
            }
        }, TaskScheduler.Default);

        globalCompletion.Task.ContinueWith
            (
            static (task, state) =>
            {
                ((AsyncCallback)state)(task);
            },
            state: callback
            );

        return globalCompletion.Task;
    }

    static Task InternalExecuteNonQueryAsync()
    {
        TaskCompletionSource source = new TaskCompletionSource();

        var returnTask = source.Task;

        returnTask = returnTask.ContinueWith(
            static (task, state) =>
            {
                return task;
            },
            state: null)
            .Unwrap();

        var context = new Context(source);

        Task.Factory.FromAsync(
            static (callback, stateObject) => BeginExecuteNonQueryInternal(callback),
            static result =>
            {
                Exception ex = ((Task)result).Exception;
                if (ex != null)
                {
                    Console.WriteLine($"endMethod - {ex.InnerException.Message}");
                    throw ex.InnerException;
                }
                else
                {
                    Console.WriteLine("endMethod");
                }
            },
            state: context)
            .ContinueWith(
            (task, state) =>
            {
                var ctx = (Context)state;
                var s = ctx.source;

                if (task.IsFaulted)
                {
                    s.SetException(task.Exception.InnerException);
                }
                else if (task.IsCanceled)
                {
                    s.SetCanceled();
                }
                else
                {
                    s.SetResult();
                }
            },
            state: context,
            scheduler: TaskScheduler.Default);

        return returnTask;
    }

    private static void Main(string[] args)
    {
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            e.SetObserved();

            if (e.Exception.InnerException != null)
            {
                Console.WriteLine("ThreadSchedulerUnobservedException " + e.Exception.InnerException.Message);
            }
            else
            {
                Console.WriteLine("ThreadSchedulerUnobservedException " + e.Exception.Message);
            }
        };

        Task.Run(async () =>
        {
            try
            {
                await ExecuteWithFailover(async () =>
                {
                    await ExecuteImplAsync();
                });
            }
            catch
            {
                // failover retry
            }
        });

        while (true)
        {
            GC.Collect();

            Thread.Sleep(100);
        }
    }
}