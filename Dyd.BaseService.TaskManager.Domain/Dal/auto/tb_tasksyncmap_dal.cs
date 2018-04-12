using Dyd.BaseService.TaskManager.Domain.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using XXF.Db;
using XXF.Extensions;

namespace Dyd.BaseService.TaskManager.Domain.Dal
{
    public partial class tb_tasksyncmap_dal
    {
        public virtual bool Add(DbConn PubConn, tb_tasksyncmap_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//
					new ProcedureParameter("@fromtaskid",    model.fromtaskid),
					//
					new ProcedureParameter("@totaskid",    model.totaskid),
					//
					new ProcedureParameter("@createtime",    model.createtime)
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_tasksyncmap(fromtaskid,totaskid,createtime)
										   values(@fromtaskid,@totaskid,@createtime)", Par);
            return rev == 1;

        }

        public virtual bool Edit(DbConn PubConn, tb_tasksyncmap_model model)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                    
					//
					new ProcedureParameter("@fromtaskid",    model.fromtaskid),
					//
					new ProcedureParameter("@totaskid",    model.totaskid),
					//
					new ProcedureParameter("@createtime",    model.createtime),
            };
            Par.Add(new ProcedureParameter("@id", model.id));

            int rev = PubConn.ExecuteSql("update tb_tasksyncmap set fromtaskid=@fromtaskid,totaskid=@totaskid,createtime=@createtime where id=@id", Par);
            return rev == 1;

        }

        public virtual bool Delete(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));

            string Sql = "delete from tb_tasksyncmap where id=@id";
            int rev = PubConn.ExecuteSql(Sql, Par);
            if (rev == 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public virtual tb_tasksyncmap_model Get(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select s.* from tb_tasksyncmap s where s.id=@id");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
        }

        public virtual tb_tasksyncmap_model CreateModel(DataRow dr)
        {
            var o = new tb_tasksyncmap_model();

            //
            if (dr.Table.Columns.Contains("id"))
            {
                o.id = dr["id"].Toint();
            }
            //
            if (dr.Table.Columns.Contains("taskid"))
            {
                o.fromtaskid = dr["fromtaskid"].Toint();
            }
            //
            if (dr.Table.Columns.Contains("totaskid"))
            {
                o.totaskid = dr["totaskid"].Toint();
            }
            //
            if (dr.Table.Columns.Contains("createtime"))
            {
                o.createtime = dr["createtime"].ToDateTime();
            }
            return o;
        }
    }
}
