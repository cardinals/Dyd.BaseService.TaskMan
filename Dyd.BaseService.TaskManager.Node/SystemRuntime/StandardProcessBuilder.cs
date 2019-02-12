using System.Diagnostics;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public  class StandardProcessBuilder :ProcessBuilderBase
    {
        
        public Process StartProcess(ProcessStartupParam param)
        {
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