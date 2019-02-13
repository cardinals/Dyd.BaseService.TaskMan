using System.Diagnostics;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public  class StandardProcessBuilder :ProcessBuilderBase
    {
        
        public override Process StartProcess()
        {
            ProcessStartupParam param = StartupParam;
            return   new Process
            {
                StartInfo = new ProcessStartInfo
                {

                    FileName = param.FileName,//fileinstallmainclassdllpath,
                    Arguments = param.Config,
                    WorkingDirectory =param.WorkDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
        }
    }
}