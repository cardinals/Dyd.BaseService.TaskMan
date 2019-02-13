using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Dyd.BaseService.TaskManager.Node.Tools;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public abstract class ProcessBuilderBase : IProcessBuilder

    {
        public ProcessStartupParam StartupParam { get; set; }

        public abstract Process StartProcess();
        
      
      

        public virtual void GetMainFileName()

        {
            
        }

        public virtual string GetService()
        {
            return Path.GetFileNameWithoutExtension(StartupParam.FileName);
        }

        public virtual string GetAssemblyVersion()
        {
            string version = string.Empty;
            if (System.IO.File.Exists(StartupParam.FileName))
            {
                try
                {
                    byte[] buffer = System.IO.File.ReadAllBytes(StartupParam.FileName);//?????LoadFile????????????????????????????????????????????
                    var assembly = Assembly.Load(buffer);
                    Regex reg = new Regex("Version=([^,]+)");
                    var m = reg.Match(assembly.FullName);
                    if (m.Success && m.Groups.Count > 1)
                    {
                        version = m.Groups[1].Value;
                    }

                    assembly = null;
                }
                catch (Exception ex)
                {
                    LogHelper.AddNodeError("ªÒ»°∞Ê±æ∫≈ ß∞‹", ex);
                }
            }
            return version;
        }
    }
}