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
    public partial class tb_businessversion_dal
    {
        /// <summary>
        /// 获取最新的版本
        /// </summary>
        /// <param name="PubConn"></param>
        /// <returns></returns>
        public tb_businessversion_model GetLatestVersion(DbConn PubConn)
        {
            return SqlHelper.Visit(ps =>
            {
                string sql = "SELECT TOP 1 * FROM[ky_monitor].[dbo].[tb_businessversion] ORDER BY id DESC";
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, sql, ps.ToParameters());
                if (ds.Tables[0].Rows.Count > 0)
                {
                    tb_businessversion_model model = CreateModel(ds.Tables[0].Rows[0]);
                    return model;
                }
                return null;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PubConn"></param>
        /// <returns></returns>
        public List<tb_businessversion_model> GetList(DbConn PubConn, int topCount = 20)
        {
            List<tb_businessversion_model> model = new List<tb_businessversion_model>();

            DataSet dsList = SqlHelper.Visit<DataSet>(ps =>
            {
                string sql = $"SELECT top {topCount} * FROM [ky_monitor].[dbo].[tb_businessversion] ORDER BY ID DESC";
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, sql, ps.ToParameters());
                return ds;
            });
            foreach (DataRow dr in dsList.Tables[0].Rows)
            {
                tb_businessversion_model m = CreateModel(dr);
                model.Add(m);
            }
            return model;
        }
    }
}
