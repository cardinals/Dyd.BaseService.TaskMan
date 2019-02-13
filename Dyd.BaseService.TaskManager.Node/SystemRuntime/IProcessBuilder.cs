using System.Diagnostics;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public interface IProcessBuilder
    {
        Process StartProcess();
       string GetAssemblyVersion();
        ProcessStartupParam StartupParam { get; set; }
        /// <summary>
        /// 按文件模板取出文件名
        /// </summary>
        void GetMainFileName();

        string GetService();
    }
}