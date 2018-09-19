using System.Runtime.CompilerServices;
using Dyd.BaseService.TaskManager.MonitorTasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Xunit.Sdk;

namespace Dyd.BaseService.TaskManger.MonitorTaskTests
{
    [TestClass]
    public class SendTests
    {
        [TestMethod]
        public void TestSend()
        {
            TaskManageErrorSendTask t = new TaskManageErrorSendTask();
            t.TestRun();
        }

    }
}