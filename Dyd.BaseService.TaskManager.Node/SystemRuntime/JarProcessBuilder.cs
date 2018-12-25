using System;
using System.Diagnostics;
using Dyd.BaseService.TaskManager.Node.Tools;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public class JarProcessBuilder : IProcessBuilder
    {
        public  Process StartProcess(ProcessStartupParam parm)
        {
            //
          //  string args= $" -jar {parm.FileName} --config {parm.Config} " ;

            string url= parm.AppConfig["service_url"];
            Uri uri = null;
            try
            {
                uri= new Uri(url);
            }
            catch
            {
                string err = $"{url}不是正确的格式";
                LogHelper.AddTaskLog(err,parm.TaskModel.id);
                throw  new Exception(err);
            }
            

            string args= $" -jar {parm.FileName} --server.port={uri.Port} " ;
            string fileName = GlobalConfig.JavaPath + @"\java ";
            LogHelper.AddTaskLog($"start:{fileName} {args}",parm.TaskModel.id);

            var result = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName =fileName, //fileinstallmainclassdllpath,

                    Arguments =args,
                    UseShellExecute = false,
                    WorkingDirectory =parm.WorkDir,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            return result;
        }
    }
}