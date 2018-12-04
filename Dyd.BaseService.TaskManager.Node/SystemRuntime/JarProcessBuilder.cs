using System.Diagnostics;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public class JarProcessBuilder : IProcessBuilder
    {
        public  Process StartProcess(ProcessStartupParam parm)
        {
            //



            var result = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GlobalConfig.JavaPath + @"\\java", //fileinstallmainclassdllpath,

                    Arguments = $" -jar {parm.FileName} --config {parm.Config} ",
                    UseShellExecute = false,
                    WorkingDirectory = parm.Config,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            return result;
        }
    }
}