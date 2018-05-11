﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    /// <summary>
    /// 任务操作提供者
    /// 提供任务的开始，关闭,重启，卸载
    /// </summary>
    public class TaskProvider
    {
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
                    taskruntimeinfo.TaskVersionModel = versiondal.GetCurrentVersion(c, taskid, taskruntimeinfo.TaskModel.taskversion);
                });
            string filelocalcachepath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\"+GlobalConfig.TaskDllCompressFileCacheDir + @"\" + taskruntimeinfo.TaskModel.id + @"\" + taskruntimeinfo.TaskModel.taskversion + @"\" +
                taskruntimeinfo.TaskVersionModel.zipfilename;
            string fileinstallpath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\"+GlobalConfig.TaskDllDir + @"\" +  taskruntimeinfo.TaskModel.id ;
            string fileinstallmainclassdllpath = fileinstallpath + @"\" + taskruntimeinfo.TaskModel.taskmainclassdllfilename;
            string taskshareddlldir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\" + GlobalConfig.TaskSharedDllsDir;
             
            XXF.Common.IOHelper.CreateDirectory(filelocalcachepath);
            XXF.Common.IOHelper.CreateDirectory(fileinstallpath);
            System.IO.File.WriteAllBytes(filelocalcachepath, taskruntimeinfo.TaskVersionModel.zipfile);

            CompressHelper.UnCompress(filelocalcachepath, fileinstallpath);
            //拷贝共享程序集
            XXF.Common.IOHelper.CopyDirectory(taskshareddlldir, fileinstallpath);
            if(taskruntimeinfo.TaskModel.task_type==TaskType.Service.Code)
            {
                try
                {


                    Process result = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {

                            FileName = fileinstallmainclassdllpath,
                            Arguments = taskruntimeinfo.TaskModel
                                .taskappconfigjson,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    result.Start();
                    while (!result.StandardOutput.EndOfStream)
                    {
                        string line = result.StandardOutput.ReadLine();
                        // do something with line
                        LogHelper.AddTaskLog(line, taskid);
                    }

                   
                }

                catch (Exception ex)
                {
                    LogHelper.AddTaskLog(ex.Message,taskid);
                    return false;
                }

                SqlHelper.ExcuteSql(GlobalConfig.TaskDataBaseConnectString, (c) =>
                {
                    tb_task_dal taskdal = new tb_task_dal();
                    //更新类型 
                    taskdal.Edit(c, taskruntimeinfo.TaskModel);
                    taskdal.UpdateTaskState(c, taskid, (int) EnumTaskState.Running);
                });
                LogHelper.AddTaskLog("节点开启任务成功", taskid);
                return true;

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
                LogHelper.AddTaskError("任务执行失败",taskid,ex);

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

            var r= DisposeTask(taskid, taskruntimeinfo,false);

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
                    taskruntimeinfo.Process.Kill();
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
            });
            LogHelper.AddTaskLog("节点卸载任务成功", taskid);
            return r;
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
