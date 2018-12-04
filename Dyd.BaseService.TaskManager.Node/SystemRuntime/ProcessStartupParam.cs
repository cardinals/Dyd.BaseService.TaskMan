using Dyd.BaseService.TaskManager.Domain.Model;


namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public class ProcessStartupParam
    {
     
        public string Flag { get;  set; }
        public string FileName { get; set; }
        public string Config { get;  set; }
        public string WorkDir { get;set; }
        public string Cron { get; set; }
        public string NameSpace { get; set; }
        public string TaskDbConnection { get; set; }
        public tb_task_model TaskModel { get; set; }
    }
}