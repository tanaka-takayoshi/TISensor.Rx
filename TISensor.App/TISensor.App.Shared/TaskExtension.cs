using System.Diagnostics;
using System.Threading.Tasks;

namespace TISensor.App
{
    public static class TaskExtension
    {
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(x => Debug.WriteLine(x.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
