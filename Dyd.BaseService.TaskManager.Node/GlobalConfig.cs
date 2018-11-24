using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyd.BaseService.TaskManager.Node
{
    /// <summary>
    /// 全局配置
    /// </summary>
    public static class GlobalConfig
    {
        /// <summary>
        /// 任务数据库连接
        /// </summary>
        public static string TaskDataBaseConnectString { get; set; }
        /// <summary>
        /// 当前节点标识
        /// </summary>
        public static int NodeID { get; set; }
        /// <summary>
        /// 任务调度平台web url地址
        /// </summary>
        public static string TaskManagerWebUrl { get { return System.Configuration.ConfigurationManager.AppSettings["TaskManagerWebUrl"]; } }

        public static string Consule => System.Configuration.ConfigurationManager.AppSettings["consul"];
        public static string JavaPath => System.Configuration.ConfigurationManager.AppSettings["java_bin"];
        public static string CronShell=> System.Configuration.ConfigurationManager.AppSettings["cron_shell"];

        /// <summary>
        /// 任务dll根目录
        /// </summary>
        public static string TaskDllDir = "任务dll根目录";
        /// <summary>
        /// 任务dll本地版本缓存
        /// </summary>
        public static string TaskDllCompressFileCacheDir = "任务dll版本缓存";
        /// <summary>
        /// 任务平台共享程序集
        /// </summary>s
        public static string TaskSharedDllsDir = "任务dll共享程序集";

        public static string TaskShellDir = "shell";
        /// <summary>
        /// 任务平台节点使用的监控插件
        /// </summary>
        public static List<SystemMonitor.BaseMonitor> Monitors = new List<SystemMonitor.BaseMonitor>();
        
    }
}
