using System;
using System.Diagnostics;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using System.Text;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public static class ProcessStart
    {
        public static Process Load(ProcessStarupParam parm)
        {
            /* const uint NORMAL_PRIORITY_CLASS = 0x0020;
             string args=$" -jar {fileName}";
             string app = fileName;
             WinApi.PROCESS_INFORMATION pInfo = new WinApi.PROCESS_INFORMATION();
             WinApi.STARTUPINFO sInfo = new WinApi.STARTUPINFO(); 
             WinApi.SECURITY_ATTRIBUTES pSec = new WinApi.SECURITY_ATTRIBUTES();
             WinApi.SECURITY_ATTRIBUTES tSec = new WinApi.SECURITY_ATTRIBUTES();
             pSec.nLength = Marshal.SizeOf(pSec);
             tSec.nLength = Marshal.SizeOf(tSec);
             bool retValue;
             retValue =  WinApi.CreateProcess(app, args,ref pSec,ref tSec,false,NORMAL_PRIORITY_CLASS,
                 IntPtr.Zero,null,ref sInfo,out pInfo);*/
            switch (parm.Flag)
            {



                case "jar":

                    return StarJarProcess(parm.FileName,parm.Config, parm.WorkDir);

                //return pInfo.dwProcessId;
                case "cron":
                    return StartCronProcess(parm.FileName,parm.WorkDir);

            }

            return null;
        }

        private static Process StartCronProcess(string fileName,string workDir)
        {
            XXF.Common.IOHelper.CopyDirectory(GlobalConfig.CronShell, workDir);
            var result = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "FastFish.Cron ", //fileinstallmainclassdllpath,

                    Arguments = $" -jar {fileName} ",
                    UseShellExecute = false,
                    WorkingDirectory = workDir,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            return result;
        }

        private static Process StarJarProcess(string fileName,string config, string workDir)
        {
            //
           
         
           
            var result = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GlobalConfig.JavaPath + @"\\java", //fileinstallmainclassdllpath,

                    Arguments = $" -jar {fileName} --config {config} ",
                    UseShellExecute = false,
                    WorkingDirectory = workDir,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            return result;
        }
    }
}