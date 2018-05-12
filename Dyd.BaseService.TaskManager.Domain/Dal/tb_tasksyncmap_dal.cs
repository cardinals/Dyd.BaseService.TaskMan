using Dyd.BaseService.TaskManager.Domain.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using XXF.Db;
using XXF.ProjectTool;

namespace Dyd.BaseService.TaskManager.Domain.Dal
{
    public partial class tb_tasksyncmap_dal
    {
        public List<tb_tasksyncmap_model> GetList(DbConn PubConn)
        {
            List<tb_tasksyncmap_model> model = new List<tb_tasksyncmap_model>();

            DataSet dsList = SqlHelper.Visit<DataSet>(ps =>
            {
                string sql = "SELECT * FROM [ky_monitor].[dbo].[tb_tasksyncmap]";
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, sql, ps.ToParameters());
                return ds;
            });
            foreach (DataRow dr in dsList.Tables[0].Rows)
            {
                tb_tasksyncmap_model m = CreateModel(dr);
                model.Add(m);
            }
            return model;
        }
    }
}
