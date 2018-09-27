using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Dyd.BaseService.TaskManager.Core;
using Dyd.BaseService.TaskManager.Domain;
using Dyd.BaseService.TaskManager.Domain.Dal;
using Dyd.BaseService.TaskManager.Domain.Model;
using Dyd.BaseService.TaskManager.Node.Tools;
using XXF.BaseService.TaskManager;
using XXF.BaseService.TaskManager.SystemRuntime;
using XXF.ProjectTool;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Consul;
using ServiceStack.Text;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
   
    /// <summary>
    /// 任务操作提供者
    /// 提供任务的开始，关闭,重启，卸载
    /// </summary>
    public class TaskProvider
    {
        private readonly ConsulRegisterManger _consulRegisterMgr=new ConsulRegisterManger();

        /// <summary>
        /// 任务的开启
        /// </summary>
        /// <param name="taskid"></param>
        /// <returns></returns>
        public bool Start(int taskid)
        {
            var taskruntimeinfo = TaskPoolManager.CreateInstance().Get(taskid.ToString());
            if (taskruntimeinfo != null)
            {
                throw new Exception("任务已在运行中");
            }
            
            taskruntimeinfo = new NodeTaskRuntimeInfo();
            taskruntimeinfo.TaskLock = new TaskLock();
            SqlHelper.ExcuteSql(GlobalConfig.TaskDataBaseConnectString, (c) =>
                {
                    tb_task_dal taskdal = new tb_task_dal();
                    taskruntimeinfo.TaskModel = taskdal.Get(c, taskid);
                    tb_version_dal versiondal = new tb_version_dal();
                    taskruntimeinfo.TaskVersionModel = versiondal.GetCurrentVersion(c, taskid,taskruntimeinfo.TaskModel.taskversion );
                    //taskruntimeinfo.ProcessId=taskdal.GetProcess(c, taskid);
                });
            //如果异常退出，进程后没有更新
            /*if (taskruntimeinfo.TaskModel.task_type == TaskType.Service.Code)
            {

            }*/
            string filelocalcachepath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\" + GlobalConfig.TaskDllCompressFileCacheDir + @"\" + taskruntimeinfo.TaskModel.id + @"\" + taskruntimeinfo.TaskModel.taskversion + @"\" +
                taskruntimeinfo.TaskVersionModel.zipfilename;
            string fileinstallpath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\" + GlobalConfig.TaskDllDir + @"\" + taskruntimeinfo.TaskModel.id;
            string fileinstallmainclassdllpath = fileinstallpath + @"\" + taskruntimeinfo.TaskModel.taskmainclassdllfilename;
            string taskshareddlldir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\" + GlobalConfig.TaskSharedDllsDir;
            string shelldlldir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\" + GlobalConfig.TaskShellDir;

            XXF.Common.IOHelper.CreateDirectory(filelocalcachepath);
            XXF.Common.IOHelper.CreateDirectory(fileinstallpath);
           // File.WriteAllBytes(filelocalcachepath, taskruntimeinfo.TaskVersionModel.zipfile);
            FileStream fs=new FileStream(filelocalcachepath,FileMode.Create,FileAccess.ReadWrite);
            SqlHelper.ExcuteSql(GlobalConfig.TaskDataBaseConnectString, (c) =>
            {
                tb_version_dal versiondal = new tb_version_dal();
                versiondal.FillByteToFile(c, taskid, taskruntimeinfo.TaskModel.taskversion, fs);
            });
            fs.Close(); 
            if (Directory.Exists(fileinstallpath))
            {
                File.SetAttributes(fileinstallpath, FileAttributes.Normal);
                Directory.Delete(fileinstallpath, true);
            }
            CompressHelper.UnCompress(filelocalcachepath, fileinstallpath);
            //拷贝共享程序集

            XXF.Common.IOHelper.CopyDirectory(taskshareddlldir, fileinstallpath);
            //如果是module 
            
            
            
            
            
            
            
            
            
            
            
            
            
            if(taskruntimeinfo.TaskModel.IsModule)
            {
                XXF.Common.IOHelper.CopyDirectory(shelldlldir,fileinstallpath);
            }
            LogHelper.AddTaskLog($"原程序集版本：{taskruntimeinfo.TaskVersionModel.assemblyversion}", taskid);
            //LogHelper.AddTaskLog($"程序集文件：{fileinstallmainclassdllpath}",taskid);
            string assemblyVersion = GetAssemblyVersion(fileinstallmainclassdllpath);
            byte[] bytes = Encoding.Default.GetBytes(taskruntimeinfo.TaskModel
                .taskappconfigjson);
            string jsonConfig=Convert.ToBase64String(bytes);
            if (taskruntimeinfo.TaskModel.task_type==TaskType.Service.Code)
            {
                bool is_module=taskruntimeinfo.TaskModel.IsModule;
                //当
                //
                try
                {
                    Process result;
                    if (!is_module)
                    {
                        string flag = taskruntimeinfo.TaskModel.ServiceFlag;
                        if (!string.IsNullOrEmpty(flag))
                        {

                            result = ProcessStart.Load(flag,fileinstallmainclassdllpath, jsonConfig, fileinstallpath);
                        }

                        else
                        {

                            result = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {

                                    FileName = fileinstallmainclassdllpath,
                                    Arguments = jsonConfig,
                                    WorkingDirectory = fileinstallpath,
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    CreateNoWindow = true
                                }
                            };
                        }
                    }
                    else
                    {
                        string shell = fileinstallpath + @"\" + "FastFish.Service.Shell.exe";
                        result = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                
                                FileName = shell, //fileinstallmainclassdllpath,
                                
                                Arguments = $"--run {fileinstallmainclassdllpath} --args {jsonConfig}",
                                UseShellExecute = false,
                                WorkingDirectory = fileinstallpath,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true
                                
                                
                            }
                            
                        };
                    }
                    
                    taskruntimeinfo.Process = result;
                  /*  AppDomain.CurrentDomain.DomainUnload += (s, e) =>
                    {
                        result.Kill();
                        result.WaitForExit();
                    };
                    AppDomain.CurrentDomain.ProcessExit += (s, e) =>
                    {
                        result.Kill();
                        result.WaitForExit();
                    };
                    AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                    {
                        result.Kill();
                        result.WaitForExit();
                    };
                  */
                  //  Task a = Task.Factory.StartNew(() =>
                   // {

                    bool isStart = result.Start();
                    try
                    {
                        ChildProcessTracker.AddProcess(result);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.AddTaskLog($"节点开启任务失败{ex.Message}", taskid);
                        //LogHelper.AddNodeError();
                        //Console.WriteLine(e);
                        //throw;
                    }
                    
                    
                   
                    Task.Factory.StartNew(() =>
                    {
                        while (!result.StandardOutput.EndOfStream)
                        {
                            string line = result.StandardOutput.ReadLine();
                            // do something with line
                            LogHelper.AddTaskLog(line, taskid);
                        }
                    });
                    // };



                }
                catch (Exception ex)
                {
                    LogHelper.AddTaskLog($"节点开启任务失败{ex.Message}", taskid);
                    throw;
                }
                bool r = TaskPoolManager.CreateInstance().Add(taskid.ToString(), taskruntimeinfo);
                SqlHelper.ExcuteSql(GlobalConfig.TaskDataBaseConnectString, (c) =>
                {
                    tb_task_dal taskdal = new tb_task_dal();
                    //更新类型 
                    taskdal.Edit(c, taskruntimeinfo.TaskModel);
                    taskdal.UpdateTaskState(c, taskid, (int) EnumTaskState.Running);
                    taskdal.UpdateProcess(c, taskid, taskruntimeinfo.Process.Id);
                    //程序集版本更新
                    if (!string.IsNullOrEmpty(assemblyVersion))
                    {
                        if (taskruntimeinfo.TaskVersionModel.assemblyversion != assemblyVersion)
                        {
                            taskruntimeinfo.TaskVersionModel.assemblyversion = assemblyVersion;
                            tb_version_dal versiondal = new tb_version_dal();
                            versiondal.UpdateAssemblyVersion(c, taskruntimeinfo.TaskVersionModel.id, assemblyVersion);
                         
                        }
                    }
                });
                //
                ConsulRegisteration item=_consulRegisterMgr.Parse(taskruntimeinfo.TaskModel);
                _consulRegisterMgr.Register(item);
                LogHelper.AddTaskLog("节点开启任务成功", taskid);
                return r;

            }
            else
            {



                try
                {
                    var dlltask = new AppDomainLoader<BaseDllTask>().Load(fileinstallmainclassdllpath,
                        taskruntimeinfo.TaskModel.taskmainclassnamespace, out taskruntimeinfo.Domain);
                    //  dlltask.Domain = taskruntimeinfo.Domain;
                    var sdktaskmodel = new XXF.BaseService.TaskManager.model.tb_task_model();
                    PropertyHelper.Copy(taskruntimeinfo.TaskModel, sdktaskmodel);
                    dlltask.SystemRuntimeInfo = new TaskSystemRuntimeInfo()
                    {
                        TaskConnectString = GlobalConfig.TaskDataBaseConnectString,
                        TaskModel = sdktaskmodel
                    };

                    dlltask.AppConfig = new TaskAppConfigInfo();
                    if (!string.IsNullOrEmpty(taskruntimeinfo.TaskModel.taskappconfigjson))
                    {
                        dlltask.AppConfig =
                            new XXF.Serialization.JsonHelper().Deserialize<TaskAppConfigInfo>(taskruntimeinfo.TaskModel
                                .taskappconfigjson);
                    }

                    taskruntimeinfo.DllTask = dlltask;
                 /*   if (dlltask is IMicroService)
                    {
                        taskruntimeinfo.TaskModel.task_type = TaskType.Service.Code;
                    }
                    else
                    {
                        taskruntimeinfo.TaskModel.task_type = TaskType.Task.Code;

                    }*/

                    bool r = TaskPoolManager.CreateInstance().Add(taskid.ToString(), taskruntimeinfo);
                    SqlHelper.ExcuteSql(GlobalConfig.TaskDataBaseConnectString, (c) =>
                    {
                        tb_task_dal taskdal = new tb_task_dal();
                        //更新类型 
                        taskdal.Edit(c, taskruntimeinfo.TaskModel);
                        taskdal.UpdateTaskState(c, taskid, (int) EnumTaskState.Running);
                        //程序集版本更新
                        if (!string.IsNullOrEmpty(assemblyVersion))
                        {
                            if (taskruntimeinfo.TaskVersionModel.assemblyversion != assemblyVersion)
                            {
                                taskruntimeinfo.TaskVersionModel.assemblyversion = assemblyVersion;
                                tb_version_dal versiondal = new tb_version_dal();
                                versiondal.UpdateAssemblyVersion(c, taskruntimeinfo.TaskVersionModel.id, assemblyVersion);
                            }
                        }
                    });
                   
                    LogHelper.AddTaskLog("节点开启任务成功", taskid);
                    return r;
                }
                catch (Exception exp)
                {
                    DisposeTask(taskid, taskruntimeinfo, true);
                    throw exp;
                }
            }
        }

       

        /// <summary>
        /// 获取程序集版本
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetAssemblyVersion(string fileName)
        {
            string version = string.Empty;
            if (System.IO.File.Exists(fileName))
            {
                try
                {
                    byte[] buffer = System.IO.File.ReadAllBytes(fileName);//不要用LoadFile去加载，那样会锁住文件，在下次启动时对文件覆盖就被占用
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
                    LogHelper.AddNodeError("获取程序集版本异常", ex);
                }
            }
            return version;
        }

        public bool Run(int taskid)
        {
            var taskruntimeinfo = TaskPoolManager.CreateInstance().Get(taskid.ToString());
            if (taskruntimeinfo == null)
            {
                throw new Exception("任务不在运行中");
            }
            try
            {

                taskruntimeinfo.DllTask.TryRun();
                LogHelper.AddTaskLog("任务执行成功", taskid);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.AddTaskError("任务执行失败", taskid, ex);

                return false;

            }


        }

        /// <summary>
        /// 任务的关闭
        /// </summary>
        /// <param name="taskid"></param>
        /// <returns></returns>
        public bool Stop(int taskid)
        {
            var taskruntimeinfo = TaskPoolManager.CreateInstance().Get(taskid.ToString());
            if (taskruntimeinfo == null)
            {
                throw new Exception("任务不在运行中");
            }

            var r = DisposeTask(taskid, taskruntimeinfo, false);

            SqlHelper.ExcuteSql(GlobalConfig.TaskDataBaseConnectString, (c) =>
            {
                tb_task_dal taskdal = new tb_task_dal();
                taskdal.UpdateTaskState(c, taskid, (int)EnumTaskState.Stop);
            });
            LogHelper.AddTaskLog("节点关闭任务成功", taskid);
            return r;
        }
        /// <summary>
        /// 任务的卸载
        /// </summary>
        /// <param name="taskid"></param>
        /// <returns></returns>
        public bool Uninstall(int taskid)
        {
            var taskruntimeinfo = TaskPoolManager.CreateInstance().Get(taskid.ToString());
            if (taskruntimeinfo == null)
            {
                throw new Exception("任务不在运行中");
            }

            bool r;
            if (taskruntimeinfo.TaskModel.task_type == TaskType.Service.Code)
            {
                try


                {
                    KillProcess(taskid.ToString(),taskruntimeinfo);
                    ConsulRegisteration item =_consulRegisterMgr.Parse(taskruntimeinfo.TaskModel);
                    _consulRegisterMgr.UnRegister(item);
                    r = true;
                }
                catch (Exception e)
                {
                    r = false;
                }
                LogHelper.AddTaskLog("节点卸载任务成功", taskid);
            }
            else
            {

               r= DisposeTask(taskid, taskruntimeinfo, true);
            }

            SqlHelper.ExcuteSql(GlobalConfig.TaskDataBaseConnectString, (c) =>
            {
                tb_task_dal taskdal = new tb_task_dal();
                taskdal.UpdateTaskState(c, taskid, (int)EnumTaskState.Stop);
                //update proc id
                taskdal.UpdateProcess(c,taskid,-1);
            });
            LogHelper.AddTaskLog("节点卸载任务成功", taskid);
            return r;
        }

        private static void KillProcess(string taskId,NodeTaskRuntimeInfo taskruntimeinfo)
        {
            if (taskruntimeinfo.Process != null)
            {
                try
                {
                    taskruntimeinfo.Process.Kill();
                    taskruntimeinfo.Process.WaitForExit();
                }
                catch (Exception ex)
                {
                    LogHelper.AddNodeError($"kill process{ex.Message}",ex);
                }
            }
            try
            {
                TaskPoolManager.CreateInstance().Remove(taskId);
            }
            catch (Exception e)
            {
                LogHelper.AddNodeError("强制资源释放之任务池释放", e);
            }
        }

        /// <summary>
        /// 任务的资源释放
        /// </summary>
        /// <param name="taskid"></param>
        /// <param name="taskruntimeinfo"></param>
        /// <returns></returns>
        private bool DisposeTask(int taskid, NodeTaskRuntimeInfo taskruntimeinfo, bool isforceDispose)
        {
            if (taskruntimeinfo != null && taskruntimeinfo.DllTask != null)
                try { taskruntimeinfo.DllTask.Dispose(); taskruntimeinfo.DllTask = null; }
                catch (TaskSafeDisposeTimeOutException ex)
                {
                    LogHelper.AddNodeError("强制资源释放之任务资源释放", ex);
                    if (isforceDispose == false)
                        throw ex;
                }
                catch (Exception e) { LogHelper.AddNodeError("强制资源释放之任务资源释放", e); }
            if (taskruntimeinfo != null && taskruntimeinfo.Domain != null)
                try { new AppDomainLoader<BaseDllTask>().UnLoad(taskruntimeinfo.Domain); taskruntimeinfo.Domain = null; }
                catch (Exception e) { LogHelper.AddNodeError("强制资源释放之应用程序域释放", e); }
            if (TaskPoolManager.CreateInstance().Get(taskid.ToString()) != null)
                try { TaskPoolManager.CreateInstance().Remove(taskid.ToString()); }
                catch (Exception e) { LogHelper.AddNodeError("强制资源释放之任务池释放", e); }
            LogHelper.AddTaskLog("节点已对任务进行资源释放", taskid);
            return true;
        }
    }
}
