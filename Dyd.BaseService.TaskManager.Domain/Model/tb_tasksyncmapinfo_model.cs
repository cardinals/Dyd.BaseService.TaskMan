using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyd.BaseService.TaskManager.Domain.Model
{
    public class tb_tasksyncmapinfo_model
    {
        public int id { get; set; }

        public tasksyncinfo fromtask { get; set; }

        public tasksyncinfo totask { get; set; }

        public bool isdiff { get; set; }
    }

    /// <summary>
    /// 任务发布信息
    /// </summary>
    public class tasksyncinfo
    {
        public int taskid { get; set; }
        public string taskname { get; set; }
        public string nodename { get; set; }
        public int nodeid { get; set; }
        public string version { get; set; }
        public string assemblyversion { get; set; }
        public string createtime { get; set; }
    }
}
