using Dyd.BaseService.TaskManager.Node.SystemRuntime;

namespace Dyd.BaseService.TaskManager.Node.Commands
{
    public class RunTaskCommand : BaseCommand
    {
        public override void Execute()
        {
            TaskProvider tp = new TaskProvider();
            tp.Run(this.CommandInfo.taskid);
        }
    }
}