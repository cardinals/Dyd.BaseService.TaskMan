using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mime;
using System.Text;
using XXF.Db;
using Dyd.BaseService.TaskManager.Domain.Model;
using XXF.ProjectTool;
using XXF.Redis;

namespace Dyd.BaseService.TaskManager.Domain.Dal
{
    public partial class tb_version_dal
    {
        public virtual tb_version_model GetCurrentVersion(DbConn PubConn, int taskid,int version)
        {
            return SqlHelper.Visit(ps =>
            {
                ps.Add("@taskid", taskid);
                ps.Add("@version", version);
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select  s.id,s.taskid ,
          s.version ,
          s.versioncreatetime ,
          s.zipfilename ,
          s.assemblyversion from tb_version s where s.taskid=@taskid and s.version=@version");
                DataSet ds = new DataSet();
               SqlToDataSet(ds,PubConn, stringSql.ToString(), ps.ToParameters());
              
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count>0)
                {
                    var model= CreateModel(ds.Tables[0].Rows[0]);
                   // FillByte(PubConn,taskid,version, model);
                    ds.Dispose();
                  
                    return model;
                }
                return null;
            });
        }

        public void FillByte(DbConn dbConn,int taskid,int version,tb_version_model model)

        {
            string sql=$@"select s.zipfile from tb_version s where s.taskid={taskid} and s.version={version}";

            IDbConnection conn = dbConn.GetConnection();
            SqlCommand cmd=null;
            SqlDataReader rd = null;
            MemoryStream memoryStream = null;
            int bufferSize = 32*1024;   
            // Size of the BLOB buffer.
            byte[] outbyte = new byte[bufferSize];  // The BLOB byte[] buffer to be filled by GetBytes.
            long retval;                            // The bytes returned from GetBytes.
            long startIndex = 0;                    // The starting position in the BLOB output.
            try
            {

                cmd =(SqlCommand) conn.CreateCommand();


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                //img= new MemoryStream((byte[])cmd.ExecuteScalar());
               rd= cmd.ExecuteReader(CommandBehavior.SequentialAccess);

                while (rd.Read())
                {
                    // Get the publisher id, which must occur before getting the logo.
                

                    // Create a file to hold the output.
                    memoryStream = new MemoryStream();
                    var bw = new BinaryWriter(memoryStream);

                    // Reset the starting byte for the new BLOB.
                    startIndex = 0;

                    // Read the bytes into outbyte[] and retain the number of bytes returned.
                    retval = rd.GetBytes(0, startIndex, outbyte, 0, bufferSize);

                    // Continue reading and writing while there are bytes beyond the size of the buffer.
                    while (retval == bufferSize)
                    {
                        bw.Write(outbyte);
                        bw.Flush();

                        // Reposition the start index to the end of the last buffer and fill the buffer.
                        startIndex += bufferSize;
                        retval = rd.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                    }

                    // Write the remaining buffer.
                    if(retval > 0) // if file size can divide to buffer size
                        bw.Write(outbyte, 0, (int)retval); //original MSDN source had retval-1, a bug
                    bw.Flush();
                   // bw.Close();
                    //fs.Seek(0, SeekOrigin.Begin);
                    model.zipfile=new byte[memoryStream.Length];
                    model.zipfile = memoryStream.ToArray();
                    bw.Close();
                    // Close the output file.
                  
                 
                    
                    
                  
                }
                
                //img.Read(model.zipfile,0,(int)img.Length);
                
            }
            finally
            {
                rd?.Close();

                cmd?.Dispose();

                memoryStream?.Close();
                memoryStream?.Dispose();
            }


        }
        public void FillByteToFile(DbConn dbConn,int taskid,int version,FileStream fs)

        {
            string sql=$@"select s.zipfile from tb_version s where s.taskid={taskid} and s.version={version}";

            IDbConnection conn = dbConn.GetConnection();
            SqlCommand cmd=null;
            SqlDataReader rd = null;
            
            int bufferSize = 32*1024;   
            // Size of the BLOB buffer.
            byte[] outbyte = new byte[bufferSize];  // The BLOB byte[] buffer to be filled by GetBytes.
            long retval;                            // The bytes returned from GetBytes.
            long startIndex = 0;                    // The starting position in the BLOB output.
            try
            {

                cmd =(SqlCommand) conn.CreateCommand();


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                //img= new MemoryStream((byte[])cmd.ExecuteScalar());
               rd= cmd.ExecuteReader(CommandBehavior.SequentialAccess);

                while (rd.Read())
                {
                    // Get the publisher id, which must occur before getting the logo.
                

                    // Create a file to hold the output.
                  
                    var bw = new BinaryWriter(fs);

                    // Reset the starting byte for the new BLOB.
                    startIndex = 0;

                    // Read the bytes into outbyte[] and retain the number of bytes returned.
                    retval = rd.GetBytes(0, startIndex, outbyte, 0, bufferSize);

                    // Continue reading and writing while there are bytes beyond the size of the buffer.
                    while (retval == bufferSize)
                    {
                        bw.Write(outbyte);
                        bw.Flush();

                        // Reposition the start index to the end of the last buffer and fill the buffer.
                        startIndex += bufferSize;
                        retval = rd.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                    }

                    // Write the remaining buffer.
                    if(retval > 0) // if file size can divide to buffer size
                        bw.Write(outbyte, 0, (int)retval); //original MSDN source had retval-1, a bug
                    bw.Flush();
                   // bw.Close();
                    //fs.Seek(0, SeekOrigin.Begin);
                  
                    bw.Close();
                    // Close the output file.
                  
                 
                    
                    
                  
                }
                
                //img.Read(model.zipfile,0,(int)img.Length);
                
            }
            finally
            {
                rd?.Close();

                cmd?.Dispose();

            }


        }
        public void SqlToDataSet(DataSet ds, DbConn dbConn, string sql, List<ProcedureParameter> procedurePar)

        {
            IDbConnection conn = dbConn.GetConnection();
            SqlDataAdapter da = null;
            SqlCommand sqlCommand = null;
            try
            {

               sqlCommand = new SqlCommand();
                sqlCommand.CommandTimeout = 0;

                sqlCommand.Connection = (SqlConnection) conn;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = sql;
                if (procedurePar != null)
                {
                    for (int index = 0; index < procedurePar.Count; ++index)
                    {
                        SqlParameter sqlParameter = ParameterTransform(procedurePar[index]);
                        sqlCommand.Parameters.Add(sqlParameter);
                    }
                }
                da = new SqlDataAdapter() {SelectCommand = sqlCommand};
                da.Fill(ds);
            }
            finally
            {
                /*if (conn != null)
                {
                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                    conn.Dispose();
                }*/
                sqlCommand?.Dispose();
                da?.Dispose();
            }
        }

        private SqlParameter ParameterTransform(ProcedureParameter Par)
        {
            if (Par.ParType == ProcParType.Default)
                return new SqlParameter(Par.Name, Par.Value);
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = Par.Name;
            switch (Par.ParType)
            {
                case ProcParType.Char:
                    sqlParameter.SqlDbType = SqlDbType.Char;
                    break;
                case ProcParType.VarChar:
                    sqlParameter.SqlDbType = SqlDbType.VarChar;
                    break;
                case ProcParType.NVarchar:
                    sqlParameter.SqlDbType = SqlDbType.NVarChar;
                    break;
                case ProcParType.Image:
                    sqlParameter.SqlDbType = SqlDbType.Binary;
                    break;
                case ProcParType.DateTime:
                    sqlParameter.SqlDbType = SqlDbType.DateTime;
                    break;
                case ProcParType.Int16:
                    sqlParameter.SqlDbType = SqlDbType.SmallInt;
                    break;
                case ProcParType.Int32:
                    sqlParameter.SqlDbType = SqlDbType.Int;
                    break;
                case ProcParType.Int64:
                    sqlParameter.SqlDbType = SqlDbType.BigInt;
                    break;
                case ProcParType.Single:
                    sqlParameter.SqlDbType = SqlDbType.Real;
                    break;
                case ProcParType.Double:
                    sqlParameter.SqlDbType = SqlDbType.Float;
                    break;
                case ProcParType.Decimal:
                    sqlParameter.SqlDbType = SqlDbType.Decimal;
                    break;
                default:
                    throw new Exception("未知类型ProcParType：" + Par.ParType.ToString());
            }
            sqlParameter.Size = Par.Size;
            sqlParameter.Direction = Par.Direction;
            switch (Par.Direction)
            {
                case ParameterDirection.Input:
                case ParameterDirection.InputOutput:
                    if (Par.Value == null)
                    {
                        sqlParameter.Value = (object)DBNull.Value;
                        break;
                    }
                    sqlParameter.Value = Par.Value;
                    break;
            }
            return sqlParameter;
        }


        public virtual tb_version_model GetSimpleVersion(DbConn PubConn, int taskid, int version)
        {
            return SqlHelper.Visit(ps =>
            {
                ps.Add("@taskid", taskid);
                ps.Add("@version", version);
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select id,taskid,version,versioncreatetime,zipfilename,assemblyversion from tb_version(nolock) s where s.taskid=@taskid and s.version=@version");
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return CreateModel(ds.Tables[0].Rows[0]);
                }
                return null;
            });
        }

        public int GetVersion(DbConn PubConn, int taskid)
        {
            return SqlHelper.Visit(ps =>
            {
                ps.Add("taskid", taskid);
                string sql = "select max(version) version from tb_version where taskid=@taskid";
                int version = Convert.ToInt32(PubConn.ExecuteScalar(sql, ps.ToParameters()));
                return version;
            });
        }

        public List<tb_version_model> GetTaskVersion(DbConn PubConn, int taskid)
        {
            return SqlHelper.Visit(ps =>
            {
                ps.Add("@taskid", taskid);
                string sql = "select version,zipfilename from tb_version where taskid=@taskid";
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, sql, ps.ToParameters());
                List<tb_version_model> model = new List<tb_version_model>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    tb_version_model m = CreateModel(dr);
                    model.Add(m);
                }
                return model;
            });
        }

        /// <summary>
        /// 指定任务的程序集版本更新
        /// </summary>
        /// <param name="PubConn"></param>
        /// <param name="id"></param>
        /// <param name="assemblyversion"></param>
        /// <returns></returns>
        public int UpdateAssemblyVersion(DbConn PubConn, int id, string assemblyversion)
        {
            return SqlHelper.Visit(ps =>
            {
                ps.Add("@assemblyversion", assemblyversion);
                ps.Add("@id", id);
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"update tb_version set assemblyversion=@assemblyversion where id=@id");
                return PubConn.ExecuteSql(stringSql.ToString(), ps.ToParameters());
            });
        }
    }
}