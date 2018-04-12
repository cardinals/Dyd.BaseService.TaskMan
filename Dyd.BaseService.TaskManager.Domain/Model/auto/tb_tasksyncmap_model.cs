using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyd.BaseService.TaskManager.Domain.Model
{
    /// <summary>
    /// 任务同步映射(从测试同步到正式目录)
    /// </summary>
    [Serializable]
    public partial class tb_tasksyncmap_model
    {
        public int id { get; set; }
        /// <summary>
        /// 源任务Id
        /// </summary>
        public int fromtaskid { get; set; }
        /// <summary>
        /// 同步任务id
        /// </summary>
        public int totaskid { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createtime { get; set; }
    }
}
