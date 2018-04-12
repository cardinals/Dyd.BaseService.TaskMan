using Dyd.BaseService.TaskManager.Domain.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using XXF.Db;
using XXF.Extensions;

namespace Dyd.BaseService.TaskManager.Domain.Dal.auto
{
    public partial class tb_businessversion_dal
    {
        public virtual bool Add(DbConn PubConn, tb_businessversion_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//
					new ProcedureParameter("@versionid",    model.versionid),
					//
					new ProcedureParameter("@taskid",    model.taskid),
					//
					new ProcedureParameter("@businessversion",    model.businessversion),
					//
					new ProcedureParameter("@description",    model.description),
					//
					new ProcedureParameter("@createtime",    model.createtime)
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_businessversion(versionid,taskid,businessversion,description,createtime)
										   values(@versionid,@taskid,@businessversion,@description,@createtime)", Par);
            return rev == 1;

        }

        public virtual bool Edit(DbConn PubConn, tb_businessversion_model model)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                    
					//
					new ProcedureParameter("@versionid",    model.versionid),
					//
					new ProcedureParameter("@taskid",    model.taskid),
					//
					new ProcedureParameter("@businessversion",    model.businessversion),
					
					new ProcedureParameter("@description",    model.description),
					//
					new ProcedureParameter("@createtime",    model.createtime)
            };
            Par.Add(new ProcedureParameter("@id", model.id));

            int rev = PubConn.ExecuteSql("update tb_businessversion set versionid=@versionid,taskid=@taskid,businessversion=@businessversion,description=@description,createtime=@createtime where id=@id", Par);
            return rev == 1;

        }

        public virtual bool Delete(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));

            string Sql = "delete from tb_businessversion where id=@id";
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

        public virtual tb_businessversion_model Get(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select s.* from tb_businessversion s where s.id=@id");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
        }

        public virtual tb_businessversion_model CreateModel(DataRow dr)
        {
            var o = new tb_businessversion_model();

            //
            if (dr.Table.Columns.Contains("id"))
            {
                o.id = dr["id"].Toint();
            }
            //
            if (dr.Table.Columns.Contains("versionid"))
            {
                o.versionid = dr["versionid"].Toint();
            }
            //
            if (dr.Table.Columns.Contains("taskid"))
            {
                o.taskid = dr["taskid"].Toint();
            }
            //
            if (dr.Table.Columns.Contains("businessversion"))
            {
                o.businessversion = dr["businessversion"].Tostring();
            }
            //
            if (dr.Table.Columns.Contains("description"))
            {
                o.description = dr["description"].Tostring();
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
