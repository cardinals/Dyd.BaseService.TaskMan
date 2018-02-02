using System.Collections.Generic;
using System.Web.Mvc;
using Dyd.BaseService.TaskManager.Core;
using Dyd.BaseService.TaskManager.Core.Net;
using Dyd.BaseService.TaskManager.Web.Models;
using XXF.Db;
using Dyd.BaseService.TaskManager.Domain.Model;
using Dyd.BaseService.TaskManager.Domain.Dal;
using System.Web;
using System;
using System.IO;
using XXF.BasicService.CertCenter;
using System.Web.Security;
//using Webdiyer.WebControls.Mvc;

namespace Dyd.BaseService.TaskManager.Web.Controllers
{
    public class OpenApiController : BaseWebController
    {
        //
        // GET: /Api/

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetNodeConfigInfo()
        {

            NodeAppConfigInfo nodeinfo = new NodeAppConfigInfo();
            nodeinfo.NodeID = Common.GetAvailableNode();
            nodeinfo.TaskDataBaseConnectString = StringDESHelper.EncryptDES(Config.TaskConnectString, "dyd88888888");
            return Json(new { code = 1, msg = "", data = nodeinfo, total = 0 }, JsonRequestBehavior.AllowGet);
        }

        public string Ping()
        {
            return "ok";
        }

        //登录
        [HttpPost]
        public JsonResult Login(string username, string password)
        {
             try
            {

                tb_user_model user = Common.GetUser(username, password);
                if (null != user)
                {
                    if (user == null)
                    {
                        return Json(new { code = -1, message = "用户在平台中未开权限。" });
                    }
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(user.id + " " + user.username + " " + username + "," + "token" + " " + user.userrole, false, (int)FormsAuthentication.Timeout.TotalMinutes);
                    string enticket = FormsAuthentication.Encrypt(ticket);
                    HttpCookie cookieofau = new HttpCookie(FormsAuthentication.FormsCookieName, enticket);
                    Response.AppendCookie(cookieofau);
                    return Json(new { code = 0, message = "登录成功！" });
                }
                else
                {
                    return Json(new { code = -1, message = "用户在平台中未开权限。" });
                }
            }
            catch (Exception exp)
            {
                return Json(new { code = -1, message = "登陆出错,请咨询管理员。错误信息:" + exp.Message });
            }
        }
        public JsonResult Get(int taskid)
        {
            return this.Visit(Core.EnumUserRole.None, () =>
            {
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    tb_task_dal dal = new tb_task_dal();
                    tb_task_model model = dal.GetOneTask(PubConn, taskid);
                    return Json(model);
                }
            });
        }

        [HttpPost]
        public JsonResult Update(HttpPostedFileBase TaskDll, tb_task_model model, string tempdatajson)
        {

            HttpPostedFileBase file = Request.Files[0];

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
                            return Json(new { code = -1, message = "没有文件" });
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
                            return Json(new { code = -1, message = "当前任务在运行中,请停止后提交" });
                        }
                        if (change == -1)
                        {
                            model.taskversion = dalversion.GetVersion(PubConn, model.id) + 1;
                        }
                        model.taskupdatetime = DateTime.Now;
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
                        return Json(new { code = 0, message = "上传成功" });
                    }
                }
                catch (Exception exp)
                {
                    return Json(new { code = -1, message = exp.Message });
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
                        return Json(new { code = -1, message = msg });
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
                    return Json(new { code = 0, message = "程序启动或关闭成功" });
                }
            });
        }


        public JsonResult Uninstall(int id)
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

        }

    }
}
