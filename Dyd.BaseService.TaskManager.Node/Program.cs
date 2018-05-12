using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dyd.BaseService.TaskManager.Node
{
    public class Program
    {
        private static bool testing = false;//是否测试
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                if (args.Length == 1)
                {
                    GlobalConfig.NodeID = Convert.ToInt32(args[0]);
                }
                else if (args.Length == 2)
                {
                    GlobalConfig.NodeID = Convert.ToInt32(args[0]);
                    GlobalConfig.TaskDataBaseConnectString = Convert.ToString(args[1]).Replace("**", " ");
                }
            }

            if (testing)
            {
                GlobalConfig.TaskDataBaseConnectString = "server=10.4.11.12;Initial Catalog=ky_monitor;User ID=dev;Password=dev201404";
                GlobalConfig.NodeID = 361;
                XXF.Common.IOHelper.CreateDirectory(GlobalConfig.TaskSharedDllsDir + @"\");
                //CommandQueueProcessor.lastMaxID = 1;
                CommandQueueProcessor.Run();
                //注册后台监控
                GlobalConfig.Monitors.Add(new SystemMonitor.TaskRecoverMonitor());
                GlobalConfig.Monitors.Add(new SystemMonitor.TaskPerformanceMonitor());
                GlobalConfig.Monitors.Add(new SystemMonitor.NodeHeartBeatMonitor());
                GlobalConfig.Monitors.Add(new SystemMonitor.TaskStopMonitor());
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
            else
            {
               Console.WriteLine(GlobalConfig.NodeID);
                Application.Run(new NodeMain());
            }
        }
    }
}
