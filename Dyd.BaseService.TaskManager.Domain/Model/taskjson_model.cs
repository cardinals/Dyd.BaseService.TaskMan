using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyd.BaseService.TaskManager.Domain.Model
{
    /// <summary>
    /// taskjson实体
    /// 每一次版本同步时，在tb_tasksyncmap表中有映射关系的所有任务发布版本信息
    /// 若版本同步时，选择了需要同步的任务，则v为新的版本号，n为1.若未选择同步，则
    /// v还是之前的版本号，n为0
    /// </summary>
    public class taskjson_model
    {
        /// <summary>
        /// taskid
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// version
        /// </summary>
        public int v { get; set; }
        /// <summary>
        /// 是否有修改：0(未发布)1(有发布)
        /// </summary>
        public int n { get; set; }
    }
}
