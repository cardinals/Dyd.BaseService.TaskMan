using Dyd.BaseService.TaskManager.Domain.Dal;
using Dyd.BaseService.TaskManager.Domain.Model;
using Dyd.BaseService.TaskManager.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XXF.Db;

namespace Dyd.BaseService.TaskManager.Web.Tools
{
    public class TaskHelper
    {
        public static tb_task_model GetTask(int id, DbConn conn = null)
        {
            tb_task_dal dal = new tb_task_dal();
            if (conn == null)
            {
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    return dal.Get(PubConn, id);
                }
            }
            else
            {
                return dal.Get(conn, id);
            }
        }

        public static tb_node_model GetNode(int id, DbConn conn = null)
        {
            tb_node_dal dal = new tb_node_dal();
            if (conn == null)
            {
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    return dal.Get(PubConn, id);
                }
            }
            else
            {
                return dal.Get(conn, id);
            }
        }

        public static tb_version_model GetVersion(int taskid, int version, DbConn conn = null)
        {
            tb_version_dal dal = new tb_version_dal();
            if (conn == null)
            {
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    return dal.GetCurrentVersion(PubConn, taskid, version);
                }
            }
            else
            {
                return dal.GetCurrentVersion(conn, taskid, version);
            }
        }

        public static tb_version_model GetSimpleVersion(int taskid, int version, DbConn conn = null)
        {
            tb_version_dal dal = new tb_version_dal();
            if (conn == null)
            {
                using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
                {
                    PubConn.Open();
                    return dal.GetSimpleVersion(PubConn, taskid, version);
                }
            }
            else
            {
                return dal.GetSimpleVersion(conn, taskid, version);
            }
        }

        public static tb_tasksyncmap_model GetTaskSyncMap(int id)
        {
            tb_tasksyncmap_dal dal = new tb_tasksyncmap_dal();
            using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
            {
                PubConn.Open();
                return dal.Get(PubConn, id);
            }
        }

        public static tb_businessversion_model GetLatestBusinessVersion()
        {
            tb_businessversion_dal dal = new tb_businessversion_dal();
            using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
            {
                PubConn.Open();
                return dal.GetLatestVersion(PubConn);
            }
        }

        public static bool AddTaskCommand(tb_command_model model)
        {
            tb_command_dal dal = new tb_command_dal();
            using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
            {
                PubConn.Open();
                return dal.Add(PubConn, model);
            }
        }

        public static bool AddVersion(tb_version_model model)
        {
            tb_version_dal dal = new tb_version_dal();
            using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
            {
                PubConn.Open();
                return dal.Add(PubConn, model);
            }
        }

        public static int UpdateTask(tb_task_model model)
        {
            tb_task_dal dal = new tb_task_dal();
            using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
            {
                PubConn.Open();
                return dal.UpdateTask(PubConn, model);
            }
        }

        public static List<tb_tasksyncmap_model> GetTaskSyncMapList()
        {
            using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
            {
                tb_tasksyncmap_dal tasksyncdal = new tb_tasksyncmap_dal();
                return tasksyncdal.GetList(PubConn);
            }
        }

        public static List<tb_businessversion_model> GetBusinessVersionList(int topCount)
        {
            using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
            {
                tb_businessversion_dal dal = new tb_businessversion_dal();
                return dal.GetList(PubConn, topCount);
            }
        }

        public static tb_businessversion_model GetBusinessVersion(int id)
        {
            using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
            {
                tb_businessversion_dal dal = new tb_businessversion_dal();
                return dal.Get(PubConn, id);
            }
        }
    }
}