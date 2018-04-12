using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyd.BaseService.TaskManager.Domain.Model
{
    /// <summary>
    /// 发布的业务版本管理
    /// </summary>
    [Serializable]
    public partial class tb_businessversion_model
    {
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// tb_version表id
        /// </summary>
        public int versionid { get; set; }
        /// <summary>
        /// 任务id
        /// </summary>
        public int taskid { get; set; }
        /// <summary>
        /// 业务版本号(1.0)
        /// </summary>
        public string businessversion { get; set; }
        /// <summary>
        /// 发布功能描述
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createtime { get; set; }
    }
}
