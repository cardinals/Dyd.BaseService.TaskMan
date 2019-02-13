using Dyd.BaseService.TaskManager.Domain.Model;
using XXF.BaseService.TaskManager.SystemRuntime;


namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public class ProcessStartupParam
    {
     
        public string Flag { get;  set; }
        /// <summary>
        /// 主文件名
        /// </summary>
        public string FileName { get; set; }
        public string Config { get;  set; }
        public string WorkDir { get;set; }
        public string Cron { get; set; }
        public string NameSpace { get; set; }
        public string TaskDbConnection { get; set; }
        public tb_task_model TaskModel { get; set; }
        
        //public string FullFileName { get; set; }
        public string FilePatten { get; set; }

        public TaskAppConfigInfo AppConfig { get; set; }
    }
}