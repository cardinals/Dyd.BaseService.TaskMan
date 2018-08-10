using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dyd.BaseService.TaskManager.Domain.Model;
using Dyd.BaseService.TaskManager.Domain.Dal;
using XXF.Db;
using Dyd.BaseService.TaskManager.Web.Models;
using Webdiyer.WebControls.Mvc;
using System.Drawing;
using System.IO;
using Dyd.BaseService.TaskManager.Core;
using XXF.Extensions;
using Dyd.BaseService.TaskManager.Web.Tools;
using Newtonsoft.Json;

namespace Dyd.BaseService.TaskManager.Web.Controllers
{
    [AuthorityCheck]
    public class TaskController : BaseWebController
    {
        //
        // GET: /Task/

        public ActionResult Index(string taskid, string keyword, string CStime, string CEtime, int categoryid = -1, int nodeid = -1, int userid = -1, int state = -999, int pagesize = 10, int pageindex = 1)
        {
            return this.Visit(Core.EnumUserRole.None, () =>
            {
                ViewBag.taskid = taskid;
                ViewBag.keyword = keyword;
                ViewBag.CStime = CStime;
                ViewBag.CEtime = CEtime;
                ViewBag.categoryid = categoryid;
                ViewBag.nodeid = nodeid;
                ViewBag.userid = userid;
                ViewBag.state = state;
                ViewBag.pagesize = pagesize;
                ViewBag.pageindex = pageindex;

                tb_task_dal dal = new tb_task_dal();
                PagedList<tb_tasklist_model> pageList = null;
                int count = 0;
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    List<tb_tasklist_model> List = dal.GetList(PubConn, taskid, keyword, CStime, CEtime, categoryid, nodeid, userid, state, pagesize, pageindex, out count);
                    pageList = new PagedList<tb_tasklist_model>(List, pageindex, pagesize, count);
                    List<tb_node_model> Node = new tb_node_dal().GetListAll(PubConn);
                    List<tb_category_model> Category = new tb_category_dal().GetList(PubConn, "");
                    List<tb_user_model> User = new tb_user_dal().GetAllUsers(PubConn);
                    ViewBag.Node = Node;
                    ViewBag.Category = Category;
                    ViewBag.User = User;
                }
                return View(pageList);
            });
        }

        public ActionResult Add()
        {
            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    List<tb_category_model> Category = new tb_category_dal().GetList(PubConn, "");
                    List<tb_node_model> Node = new tb_node_dal().GetListAll(PubConn);
                    List<tb_user_model> User = new tb_user_dal().GetAllUsers(PubConn);
                    ViewBag.Node = Node;
                    ViewBag.Category = Category;
                    ViewBag.User = User;
                    return View();
                }
            });
        }

        [HttpPost]
        public ActionResult Add(HttpPostedFileBase TaskDll, tb_task_model model, string tempdatajson)
        {
            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                string filename = TaskDll.FileName;
                Stream dll = TaskDll.InputStream;
                byte[] dllbyte = new byte[dll.Length];
                dll.Read(dllbyte, 0, Convert.ToInt32(dll.Length));
                tb_task_dal dal = new tb_task_dal();
                tb_version_dal dalversion = new tb_version_dal();
                tb_tempdata_dal tempdatadal = new tb_tempdata_dal();
                //model.taskcreateuserid = Common.GetUserId(this);
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    model.taskcreatetime = DateTime.Now;
                    model.taskversion = 1;
                    ConvertServiceTypeToFlag(model);
                    int taskid = dal.AddTask(PubConn, model);
                    dalversion.Add(PubConn, new tb_version_model()
                    {
                        taskid = taskid,
                        version = 1,
                        versioncreatetime = DateTime.Now,
                        zipfile = dllbyte,
                        zipfilename = System.IO.Path.GetFileName(filename)
                    });
                    tempdatadal.Add(PubConn, new tb_tempdata_model()
                    {
                        taskid = taskid,
                        tempdatajson = tempdatajson,
                        tempdatalastupdatetime = DateTime.Now
                    });
                }
                return RedirectToAction("index");
            });
        }

        private static void ConvertServiceTypeToFlag(tb_task_model model)
        {
            switch (model.task_type)
            {
                case "module":
                    model.task_type = "service";
                    model.ServiceFlag = "module";
                    break;
                case "service":
                    model.task_type = "service";
                    model.ServiceFlag = string.Empty;
                    break;
            }
        }

        public ActionResult Update(int taskid)
        {
            return this.Visit(Core.EnumUserRole.None, () =>
            {
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    tb_task_dal dal = new tb_task_dal();
                    tb_task_model model = dal.GetOneTask(PubConn, taskid);
                    ConvertServiceFlagToType(model);
                    tb_tempdata_model tempdatamodel = new tb_tempdata_dal().GetByTaskID(PubConn, taskid);
                    List<tb_version_model> Version = new tb_version_dal().GetTaskVersion(PubConn, taskid);
                    List<tb_category_model> Category = new tb_category_dal().GetList(PubConn, "");
                    List<tb_node_model> Node = new tb_node_dal().GetListAll(PubConn);
                    List<tb_user_model> User = new tb_user_dal().GetAllUsers(PubConn);
                    ViewBag.Node = Node;
                    ViewBag.Category = Category;
                    ViewBag.Version = Version;
                    ViewBag.User = User;
                    ViewBag.TempData = tempdatamodel;
                    return View(model);
                }
            });
        }

        private static void ConvertServiceFlagToType(tb_task_model model)
        {
            if (model.task_type == "service")

            {
                if (model.ServiceFlag == "module")
                    model.task_type = "module";
               
            }
        }

        [HttpPost]
        public ActionResult Update(HttpPostedFileBase TaskDll, tb_task_model model, string tempdatajson)
        {
            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                try
                {
                    tb_task_dal dal = new tb_task_dal();
                    tb_version_dal dalversion = new tb_version_dal();
                    tb_tempdata_dal tempdatadal = new tb_tempdata_dal();
                    byte[] dllbyte = null;
                    string filename = "";
                    int change = model.taskversion;
                    if (change == -1)
                    {
                        if (TaskDll == null)
                        {
                            throw new Exception("没有文件！");
                        }
                        filename = TaskDll.FileName;
                        Stream dll = TaskDll.InputStream;
                        dllbyte = new byte[dll.Length];
                        dll.Read(dllbyte, 0, Convert.ToInt32(dll.Length));
                        //model.taskcreateuserid = Common.GetUserId(this);
                    }
                    using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                    {
                        PubConn.Open();
                        var task = dal.GetOneTask(PubConn, model.id);
                        if (task.taskstate == (int)Dyd.BaseService.TaskManager.Core.EnumTaskState.Running)
                        {

                            throw new Exception("当前任务在运行中,请停止后提交");
                        }
                        if (change == -1)
                        {
                            model.taskversion = dalversion.GetVersion(PubConn, model.id) + 1;
                        }
                        model.taskupdatetime = DateTime.Now;
                        ConvertServiceTypeToFlag(model);
                        dal.UpdateTask(PubConn, model);
                        if (change == -1)
                        {
                            dalversion.Add(PubConn, new tb_version_model()
                            {
                                taskid = model.id,
                                version = model.taskversion,
                                versioncreatetime = DateTime.Now,
                                zipfile = dllbyte,
                                zipfilename = System.IO.Path.GetFileName(filename)
                            });
                        }
                        tempdatadal.UpdateByTaskID(PubConn, new tb_tempdata_model()
                        {
                            taskid = model.id,
                            tempdatajson = tempdatajson,
                            tempdatalastupdatetime = DateTime.Now
                        });
                        return RedirectToAction("index");
                    }
                }
                catch (Exception exp)
                {
                    ModelState.AddModelError("", exp.Message);
                    return View();
                }
            });
        }

        public JsonResult ChangeTaskState(int id, int nodeid, int state)
        {
            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                tb_command_dal dal = new tb_command_dal();
                tb_task_dal taskDal = new tb_task_dal();
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    if (taskDal.CheckTaskState(PubConn, id) == state)
                    {
                        string msg = state == 1 ? "已开启" : "已关闭";
                        return Json(new { code = -1, msg = msg });
                    }
                    else
                    {
                        tb_command_model m = new tb_command_model()
                        {
                            command = "",
                            commandcreatetime = DateTime.Now,
                            commandname = state == (int)EnumTaskCommandName.StartTask ? EnumTaskCommandName.StartTask.ToString() : EnumTaskCommandName.StopTask.ToString(),
                            taskid = id,
                            nodeid = nodeid,
                            commandstate = (int)EnumTaskCommandState.None
                        };
                        dal.Add(PubConn, m);
                    }
                    return Json(new { code = 1, msg = "Success" });
                }
            });
        }

        public JsonResult ChangeMoreTaskState(string poststr)
        {
            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<PostChangeModel> post = new List<PostChangeModel>();
                post = jss.Deserialize<List<PostChangeModel>>(poststr);
                tb_command_dal dal = new tb_command_dal();
                tb_task_dal taskDal = new tb_task_dal();
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    foreach (PostChangeModel m in post)
                    {
                        m.state = m.state == 0 ? 1 : 0;
                        if (taskDal.CheckTaskState(PubConn, m.id) == m.state)
                        {
                            string msg = m.state == 1 ? "已开启" : "已关闭";
                            return Json(new { code = -1, msg = msg });
                        }
                        else
                        {
                            tb_command_model c = new tb_command_model()
                            {
                                command = "",
                                commandcreatetime = DateTime.Now,
                                commandname = m.state == (int)EnumTaskCommandName.StartTask ? EnumTaskCommandName.StartTask.ToString() : EnumTaskCommandName.StopTask.ToString(),
                                taskid = m.id,
                                nodeid = m.nodeid,
                                commandstate = (int)EnumTaskCommandState.None
                            };
                            dal.Add(PubConn, c);
                        }
                    }
                    return Json(new { code = 1, data = post });
                }
            });
        }

        public JsonResult CheckTaskState(int id, int state)
        {
            return this.Visit(EnumUserRole.None, () =>
            {
                tb_task_dal dal = new tb_task_dal();
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    int taskstate = dal.CheckTaskState(PubConn, id);
                    if (taskstate == state)
                    {
                        return Json(new { code = 1, msg = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { code = -1, msg = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
            });
        }

        public JsonResult Delete(int id)
        {
            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                try
                {
                    tb_task_dal dal = new tb_task_dal();
                    using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                    {
                        PubConn.Open();
                        bool state = dal.DeleteOneTask(PubConn, id);
                        return Json(new { code = 1, state = state });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { code = -1, msg = ex.Message });
                }
            });
        }

        public JsonResult Run(int id)
        {
            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                try
                {
                    tb_task_dal dal = new tb_task_dal();
                    tb_command_dal cmdDal = new tb_command_dal();
                    using (DbConn PubConn =
                     DbConfig.CreateConn(Config.TaskConnectString))
                    {
                        PubConn.Open();
                        tb_task_model task = dal.GetOneTask(PubConn, id);
                        tb_command_model c = new tb_command_model()
                        {
                            command = "",
                            commandcreatetime = DateTime.Now,
                            commandname = EnumTaskCommandName.RunTask.ToString(),
                            taskid = id,
                            nodeid = task.nodeid,
                            commandstate = (int)EnumTaskCommandState.None
                        };
                        cmdDal.Add(PubConn, c);
                        return Json(new { code = 1, msg = "Success" });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { code = -1, msg = ex.Message });
                }
            });
        }

        public JsonResult Uninstall(int id)
        {
            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                try
                {
                    tb_command_dal commanddal = new tb_command_dal();
                    tb_task_dal dal = new tb_task_dal();
                    using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                    {
                        PubConn.Open();
                        var taskmodel = dal.Get(PubConn, id);
                        dal.UpdateTaskState(PubConn, id, (int)Core.EnumTaskState.Stop);

                        tb_command_model m = new tb_command_model()
                        {
                            command = "",
                            commandcreatetime = DateTime.Now,
                            commandname = EnumTaskCommandName.UninstallTask.ToString(),
                            taskid = id,
                            nodeid = taskmodel.nodeid,
                            commandstate = (int)EnumTaskCommandState.None
                        };
                        commanddal.Add(PubConn, m);

                        return Json(new { code = 1 });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { code = -1, msg = ex.Message });
                }
            });
        }

        public ActionResult Cron()
        {
            return View();
        }

        public ActionResult CustomCorn()
        {
            return View();
        }

        public JsonResult Copy(int id)
        {
            tb_task_model model = new tb_task_model();
            tb_version_model vermodel = new tb_version_model();
            tb_tempdata_model tempdatamodel = new tb_tempdata_model();

            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                try
                {
                    tb_task_dal dal = new tb_task_dal();
                    tb_version_dal dalversion = new tb_version_dal();
                    tb_tempdata_dal tempdatadal = new tb_tempdata_dal();

                    using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                    {
                        PubConn.Open();

                        //取出需要复制的数据
                        model = dal.GetOneTask(PubConn, id);
                        tempdatamodel = tempdatadal.GetByTaskID(PubConn, id);
                        vermodel = dalversion.GetCurrentVersion(PubConn, id, model.taskversion);

                        //分别插入
                        model.taskstate = 0;
                        model.taskcreatetime = DateTime.Now;
                        model.taskversion = 1;
                        int taskid = dal.AddTask(PubConn, model);

                        vermodel.taskid = taskid;
                        vermodel.version = 1;
                        vermodel.versioncreatetime = DateTime.Now;
                        dalversion.Add(PubConn, vermodel);

                        tempdatamodel.taskid = taskid;
                        tempdatamodel.tempdatalastupdatetime = DateTime.Now;
                        tempdatadal.Add(PubConn, tempdatamodel);
                    }
                    return Json(new { code = 1, state = "复制成功" });
                }
                catch (Exception ex)
                {
                    return Json(new { code = -1, msg = ex.Message });
                }
            });
        }

        /// <summary>
        /// 同步页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Sync()
        {
            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                PagedList<tb_tasksyncmapinfo_model> pageList;
                var list = new List<tb_tasksyncmap_model>();
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    tb_tasksyncmap_dal tasksyncdal = new tb_tasksyncmap_dal();
                    list = tasksyncdal.GetList(PubConn);

                    var modelList = new List<tb_tasksyncmapinfo_model>();
                    if (list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            var fromtaskinfo = TaskHelper.GetTask(item.fromtaskid, PubConn);
                            var fromtasknodeinfo = TaskHelper.GetNode(fromtaskinfo.nodeid, PubConn);
                            var totaskinfo = TaskHelper.GetTask(item.totaskid, PubConn);
                            var totasknodeinfo = TaskHelper.GetNode(totaskinfo.nodeid, PubConn);
                            var fromtaskversioninfo = TaskHelper.GetSimpleVersion(fromtaskinfo.id, fromtaskinfo.taskversion, PubConn);
                            var totaskversioninfo = TaskHelper.GetSimpleVersion(totaskinfo.id, totaskinfo.taskversion, PubConn);
                            modelList.Add(new tb_tasksyncmapinfo_model
                            {
                                id = item.id,
                                fromtask = new tasksyncinfo { taskid = fromtaskinfo.id, taskname = fromtaskinfo.taskname, nodeid = fromtasknodeinfo.id, nodename = fromtasknodeinfo.nodename, version = fromtaskinfo.taskversion.ToString(), assemblyversion = fromtaskversioninfo.assemblyversion, createtime = fromtaskversioninfo.versioncreatetime.ToString("yyyy-MM-dd HH:mm:ss") },
                                totask = new tasksyncinfo { taskid = totaskinfo.id, taskname = totaskinfo.taskname, nodeid = totasknodeinfo.id, nodename = totasknodeinfo.nodename, version = totaskinfo.taskversion.ToString(), assemblyversion = totaskversioninfo.assemblyversion, createtime = totaskversioninfo.versioncreatetime.ToString("yyyy-MM-dd HH:mm:ss") },
                                isdiff =true //fromtaskversioninfo.assemblyversion != totaskversioninfo.assemblyversion
                            });
                        }
                    }
                    pageList = new PagedList<tb_tasksyncmapinfo_model>(modelList.Where(item => item.isdiff), 1, 1000);
                    return View(pageList);
                }
            });
        }

        /// <summary>
        /// 同步提交
        /// </summary>
        [HttpPost]
        public JsonResult Sync(string ids, string description)
        {
            List<string> idList = ids.Split(',').ToList();
            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                try
                {
                    string currentVersion = "1.0";
                    var latestVersion = TaskHelper.GetLatestBusinessVersion();
                    if (latestVersion != null)
                    {
                        double version;
                        double.TryParse(latestVersion.businessversion, out version);
                        currentVersion = (version + 0.1).ToString("F1");
                    }

                    List<taskjson_model> taskjsonList = new List<taskjson_model>();
                    List<tb_tasksyncmap_model> tasksynclist = TaskHelper.GetTaskSyncMapList();
                    foreach (var item in tasksynclist)
                    {
                        if (idList.Exists(id => id == item.id.ToString()))
                        {
                            //有些任务可能已经删除掉，就不再同步
                            var totask = TaskHelper.GetTask(item.totaskid);
                            if (totask != null)
                            {
                                var totaskversion = TaskHelper.GetVersion(totask.id, totask.taskversion);
                                var fromtask = TaskHelper.GetTask(item.fromtaskid);
                                var fromtaskversion = TaskHelper.GetVersion(fromtask.id, fromtask.taskversion);
                                //停止原来的任务
                                bool result = TaskHelper.AddTaskCommand(new tb_command_model
                                {
                                    command = "",
                                    commandcreatetime = DateTime.Now,
                                    taskid = totask.id,
                                    nodeid = totask.nodeid,
                                    commandname = EnumTaskCommandName.StopTask.ToString(),
                                    commandstate = (int)EnumTaskCommandState.None
                                });
                                //复制源任务到发布任务,并启动同步后的任务
                                var model = new tb_version_model
                                {
                                    taskid = totask.id,
                                    version = totaskversion.version + 1,
                                    versioncreatetime = DateTime.Now,
                                    zipfile = fromtaskversion.zipfile,
                                    zipfilename = fromtaskversion.zipfilename
                                };
                                TaskHelper.AddVersion(model);
                                TaskHelper.AddTaskCommand(new tb_command_model
                                {
                                    command = "",
                                    commandcreatetime = DateTime.Now,
                                    taskid = totask.id,
                                    nodeid = totask.nodeid,
                                    commandname = EnumTaskCommandName.StartTask.ToString(),
                                    commandstate = (int)EnumTaskCommandState.None
                                });
                                totask.taskversion = model.version;
                                totask.taskupdatetime = DateTime.Now;
                                totask.businessversion = currentVersion;
                                TaskHelper.UpdateTask(totask);
                                taskjsonList.Add(new taskjson_model { id = totask.id, v = model.version, n = 1 });
                            }
                        }
                        else
                        {
                            var totask = TaskHelper.GetTask(item.totaskid);
                            if (totask != null)
                            {
                                taskjsonList.Add(new taskjson_model { id = totask.id, v = totask.taskversion, n = 0 });
                            }
                        }
                    }

                    using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                    {
                        PubConn.Open();
                        tb_businessversion_dal dal = new tb_businessversion_dal();
                        var result = dal.Add(PubConn, new tb_businessversion_model
                        {
                            createtime = DateTime.Now,
                            businessversion = currentVersion,
                            description = description,
                            taskjson = JsonConvert.SerializeObject(taskjsonList)
                        });
                        if (!result)
                        {
                            return Json(new { code = -1, msg = "业务版本发布失败" });
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { code = -1, msg = ex.Message });
                }
                return Json(new { code = 1, msg = "" });
            });
        }

        /// <summary>
        /// 回滚操作页面
        /// </summary>
        /// <returns></returns>
        public ActionResult RollbackView(int id = 0)
        {
            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                var topversionlist = TaskHelper.GetBusinessVersionList(20);
                if (topversionlist.Count == 0)
                {
                    return RedirectToAction("Sync");
                }
                ViewBag.TopVersionList = topversionlist;
                PagedList<tasksyncinfo> pageList;
                var currentVersion = TaskHelper.GetLatestBusinessVersion();
                if (id > 0)
                {
                    currentVersion = TaskHelper.GetBusinessVersion(id);
                }
                ViewBag.CurrentId = id;
                var modelList = new List<tasksyncinfo>();
                if (currentVersion != null)
                {
                    ViewBag.VersionTime = currentVersion.createtime.ToString("yyyy-MM-dd HH:mm:ss");
                    ViewBag.Description = currentVersion.description;
                    var taskList = GetTaskJsonList(currentVersion.taskjson);
                    foreach (var item in taskList)
                    {
                        if (item.n == 1)
                        {
                            var taskinfo = TaskHelper.GetTask(item.id);
                            if (taskinfo != null)
                            {
                                var tasknodeinfo = TaskHelper.GetNode(taskinfo.nodeid);
                                var taskversioninfo = TaskHelper.GetSimpleVersion(item.id, item.v);
                                modelList.Add(new tasksyncinfo
                                {
                                    taskid = taskinfo.id,
                                    taskname = taskinfo.taskname,
                                    nodename = tasknodeinfo == null ? "" : tasknodeinfo.nodename,
                                    version = item.v.ToString(),
                                    assemblyversion = taskversioninfo != null ? taskversioninfo.assemblyversion : "",
                                    createtime = taskversioninfo != null ? taskversioninfo.versioncreatetime.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty,
                                    nodeid = tasknodeinfo == null ? 0 : tasknodeinfo.id
                                });
                            }
                        }
                    }
                }
                pageList = new PagedList<tasksyncinfo>(modelList, 1, 1000);
                return View("rollback", pageList);
            });
        }

        private List<taskjson_model> GetTaskJsonList(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<taskjson_model>>(json);
            }
            catch
            {
                return new List<taskjson_model>();
            }
        }


        /// <summary>
        /// 回滚
        /// </summary>
        /// <param name="versionid"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Rollback(int versionid)
        {
            return this.Visit(Core.EnumUserRole.Admin, () =>
            {
                try
                {
                    string currentVersion = "1.01";
                    var latestVersion = TaskHelper.GetLatestBusinessVersion();
                    if (latestVersion != null)
                    {
                        double version;
                        double.TryParse(latestVersion.businessversion, out version);
                        currentVersion = (version + 0.01).ToString("F2");
                    }

                    List<taskjson_model> taskjsonList = new List<taskjson_model>();
                    var versioninfo = TaskHelper.GetBusinessVersion(versionid);
                    if (versioninfo != null)
                    {
                        var taskList = GetTaskJsonList(versioninfo.taskjson);
                        foreach (var item in taskList)
                        {
                            var totask = TaskHelper.GetTask(item.id);
                            if (totask != null)
                            {
                                var totaskversion = TaskHelper.GetVersion(totask.id, totask.taskversion);
                                var fromtaskversion = TaskHelper.GetVersion(totask.id, item.v);
                                //停止原来的任务
                                bool result = TaskHelper.AddTaskCommand(new tb_command_model
                                {
                                    command = "",
                                    commandcreatetime = DateTime.Now,
                                    taskid = totask.id,
                                    nodeid = totask.nodeid,
                                    commandname = EnumTaskCommandName.StopTask.ToString(),
                                    commandstate = (int)EnumTaskCommandState.None
                                });
                                var model = new tb_version_model
                                {
                                    taskid = totask.id,
                                    version = totaskversion.version + 1,
                                    versioncreatetime = DateTime.Now,
                                    zipfile = fromtaskversion.zipfile,
                                    zipfilename = fromtaskversion.zipfilename
                                };
                                TaskHelper.AddVersion(model);
                                TaskHelper.AddTaskCommand(new tb_command_model
                                {
                                    command = "",
                                    commandcreatetime = DateTime.Now,
                                    taskid = totask.id,
                                    nodeid = totask.nodeid,
                                    commandname = EnumTaskCommandName.StartTask.ToString(),
                                    commandstate = (int)EnumTaskCommandState.None
                                });
                                totask.taskversion = model.version;
                                totask.taskupdatetime = DateTime.Now;
                                totask.businessversion = currentVersion;
                                TaskHelper.UpdateTask(totask);
                                taskjsonList.Add(new taskjson_model { id = totask.id, v = model.version, n = 1 });
                            }
                        }
                        using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                        {
                            PubConn.Open();
                            tb_businessversion_dal dal = new tb_businessversion_dal();
                            var result = dal.Add(PubConn, new tb_businessversion_model
                            {
                                createtime = DateTime.Now,
                                businessversion = currentVersion,
                                description = "【回滚操作】" + versioninfo.description,
                                taskjson = JsonConvert.SerializeObject(taskjsonList)
                            });
                            if (!result)
                            {
                                return Json(new { code = -1, msg = "回滚版本发布失败" });
                            }
                        }
                    }
                    else
                    {
                        return Json(new { code = -1, msg = "版本不存在" });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { code = -1, msg = ex.Message });
                }
                return Json(new { code = 1, msg = "回滚成功" });
            });
        }
    }
}
