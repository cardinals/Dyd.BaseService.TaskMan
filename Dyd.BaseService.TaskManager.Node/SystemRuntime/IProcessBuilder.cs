using System.Diagnostics;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public interface IProcessBuilder
    {
        Process StartProcess(ProcessStartupParam param);
    }
}