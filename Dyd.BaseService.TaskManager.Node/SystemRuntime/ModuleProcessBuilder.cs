using System;
using System.Diagnostics;
using XXF.Api;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public class ModuleProcessBuilder : ProcessBuilderBase
    {
        public override Process StartProcess()
        {
            ProcessStartupParam param = StartupParam;
            string shell = param.WorkDir + @"\" + "FastFish.Service.Shell.exe";
           var result = new Process
            {
                StartInfo = new ProcessStartInfo
                {

                    FileName = shell, //fileinstallmainclassdllpath,

                    Arguments = $"--run {param.FileName} --args {param.Config}",
                    UseShellExecute = false,
                    WorkingDirectory = param.WorkDir,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true


                }

            };
            return result;
        }
    }
}