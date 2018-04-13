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
        public static tb_task_model GetTask(int id)
        {
            tb_task_dal dal = new tb_task_dal();
            using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
            {
                PubConn.Open();
                return dal.Get(PubConn, id);
            }
        }

        public static tb_node_model GetNode(int id)
        {
            tb_node_dal dal = new tb_node_dal();
            using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
            {
                PubConn.Open();
                return dal.Get(PubConn, id);
            }
        }

        public static tb_version_model GetVersion(int taskid, int version)
        {
            tb_version_dal dal = new tb_version_dal();
            using (DbConn PubConn = DbConfig.CreateConn(Config.TaskConnectString))
            {
                PubConn.Open();
                return dal.GetCurrentVersion(PubConn, taskid, version);
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
    }
}