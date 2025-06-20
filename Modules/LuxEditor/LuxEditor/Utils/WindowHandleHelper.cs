using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using System.Threading.Tasks;

namespace Luxoria.App.Utils;

public static class WindowHandleHelper
{
    public static Task<nint> GetMainHwndAsync(IEventBus bus)
    {
        var tcs = new TaskCompletionSource<nint>();
        bus.Publish(new RequestWindowHandleEvent(h => tcs.TrySetResult(h)));
        return tcs.Task;
    }
}
