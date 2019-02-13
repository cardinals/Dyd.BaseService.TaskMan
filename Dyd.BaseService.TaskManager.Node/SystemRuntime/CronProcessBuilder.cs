using System;
using System.Diagnostics;
using System.Text;
using Dyd.BaseService.TaskManager.Node.Tools;
using ServiceStack.Text;
using XXF.BaseService.TaskManager.model;
using XXF.BaseService.TaskManager.SystemRuntime;
using XXF.ProjectTool;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public class CronProcessBuilder:ProcessBuilderBase
    {
        public override  Process StartProcess()
        {
            ProcessStartupParam param = StartupParam;
            XXF.Common.IOHelper.CopyDirectory(GlobalConfig.CronShell, param.WorkDir);
            TaskSystemRuntimeInfo info = new TaskSystemRuntimeInfo()
            {
                TaskModel = new tb_task_model(),
                TaskConnectString = param.TaskDbConnection
            };
            PropertyHelper.Copy(param.TaskModel,info.TaskModel);
            byte[] bytes = Encoding.Default.GetBytes(info.ToJson());
            string jobData = Convert.ToBase64String(bytes);
            string args= $" --cron \"{param.Cron}\" " +
                     $" --dll {param.FileName}" +
                     $" --name_space {param.NameSpace} " +
                     $" --config {param.Config} " +
                     $" --job_data {jobData}";
            LogHelper.AddTaskLog($"args:{args}",param.TaskModel.id);
            var result = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = param.WorkDir + @"\" + "FastFish.Cron.exe ", //fileinstallmainclassdllpath,
                   
                    Arguments = args,
                    UseShellExecute = false,
                    WorkingDirectory = param.WorkDir,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            return result;
        }

    }
}