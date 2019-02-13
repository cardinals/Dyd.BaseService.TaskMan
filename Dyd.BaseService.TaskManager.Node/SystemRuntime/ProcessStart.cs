using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using System.Text;
using XXF.BaseService.TaskManager.SystemRuntime;
namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public class ProcessStart
    {
        private static ProcessStart _instance;
        private StandardProcessBuilder _standardProcess=new StandardProcessBuilder();

        private Dictionary<string, IProcessBuilder> _builders = new Dictionary<string, IProcessBuilder>
        {
            {"cron", new CronProcessBuilder()},
            {"jar", new JarProcessBuilder()},
            {
                "module",new ModuleProcessBuilder()

            },
           

        };






        public static ProcessStart GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ProcessStart();
            }

            return _instance;
        }

        public Process Load(ProcessStartupParam parm)
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
            /*  switch (parm.Flag)
              {
  
  
  
                  case "jar":
  
                      return StarJarProcess(parm.FileName,parm.Config, parm.WorkDir);
  
                  //return pInfo.dwProcessId;
                  case "cron":
                      return StartCronProcess(parm.FileName,parm.WorkDir);
  
              }*/
         /*   if (!_builders.ContainsKey(parm.Flag))
            {
                //throw new Exception($"{parm.Flag}不支持");
              return   _standardProcess.StartProcess(parm);
            }

            return _builders[parm.Flag].StartProcess(parm);*/
         return null;
        }

        public IProcessBuilder GetBuilder(ProcessStartupParam parm)

        {
            IProcessBuilder result=null;
            if (!_builders.ContainsKey(parm.Flag))
            {
                //throw new Exception($"{parm.Flag}不支持");
                result=   _standardProcess;
            }
            else
            {

                result = _builders[parm.Flag];
            }

            result.StartupParam = parm;
           return result;
        }

    }
}
    