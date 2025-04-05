using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

public static class DispatcherQueueExtensions
{
    /// <summary>
    /// Enqueues an action to be executed on the dispatcher queue.
    /// </summary>
    /// <param name="dispatcherQueue"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Task EnqueueAsync(this DispatcherQueue dispatcherQueue, Action action)
    {
        var tcs = new TaskCompletionSource<bool>();

        dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                action();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    /// <summary>
    /// Enqueues a function to be executed on the dispatcher queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dispatcherQueue"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Task<T> EnqueueAsync<T>(this DispatcherQueue dispatcherQueue, Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>();

        dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                var result = func();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }
}
