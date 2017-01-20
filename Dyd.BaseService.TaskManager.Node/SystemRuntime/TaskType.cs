using System.ComponentModel;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public class TaskType
    {
        public string Code { private set; get; }

        public TaskType(string code)
        {
            Code = code;
        }
        public static  TaskType Task=new TaskType("task");
        public static  TaskType WebService=new TaskType("webservice");
        public static  TaskType Service=new TaskType("service");
    }
}