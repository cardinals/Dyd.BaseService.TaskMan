using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Dyd.BaseService.TaskManager.Node.Tools;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public class JarProcessBuilder : ProcessBuilderBase
    {
        public override Process StartProcess()
        {
            ProcessStartupParam parm = StartupParam;
            //
            //  string args= $" -jar {parm.FileName} --config {parm.Config} " ;

            string url = parm.AppConfig["service_url"];
            Uri uri = null;
            try
            {
                uri = new Uri(url);
            }
            catch
            {
                string err = $"{url}不是正确的格式";
                LogHelper.AddTaskLog(err, parm.TaskModel.id);
                throw new Exception(err);
            }


            string args = $" -jar {parm.FileName} --server.port={uri.Port} ";
            string fileName = GlobalConfig.JavaPath + @"\java ";
            LogHelper.AddTaskLog($"start:{fileName} {args}", parm.TaskModel.id);

            var result = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName, //fileinstallmainclassdllpath,

                    Arguments = args,
                    UseShellExecute = false,
                    WorkingDirectory = parm.WorkDir,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            return result;
        }

        public override void GetMainFileName()
        {
            //string patten=$"{Env.MainFile}.*.jar";
            string patten = StartupParam.FilePatten;
            string msg;

            string[] files = Directory.GetFiles(StartupParam.WorkDir, patten);
            if (files == null || files.Length < 1)
            {
                msg = $"{StartupParam.WorkDir}没有符合{patten}条件文件";
                Console.WriteLine(msg);
                throw new Exception(msg);
            }

            if (files.Length > 1)
            {
                msg = $"{StartupParam.WorkDir}没有符合{patten}条件文件多于1个,不符合要求";
                Console.WriteLine(msg);
                throw new Exception(msg);
            }

            StartupParam.FileName =Path.Combine(StartupParam.WorkDir, files[0]);


        }

        public override string GetService()
        {
            string patten=StartupParam.FilePatten;
            string service=patten.Substring(0,patten.IndexOf("-*"));
            return service;
        }

        public override string GetAssemblyVersion()
        {
            Regex reg = new Regex("(?!\\.)(\\d+(\\.\\d+)+)(?:[-.][A-Z]+)?(?![\\d.])$");
            string version = null;
            string fileName =
                Path.GetFileNameWithoutExtension(StartupParam.FileName);
            Console.WriteLine(fileName);
            var m = reg.Match(fileName);
            if (m.Success && m.Groups.Count > 1)
            {
                version = m.Groups[1].Value;
                //Console.WriteLine(version);
            }

            return version;

        }
    }

}