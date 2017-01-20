using Dyd.BaseService.TaskManager.Node.SystemRuntime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dyd.BaseService.TaskManager.NodeTests.SystemRuntime
{
    [TestClass()]
    public class TaskProviderTests
    {
        [TestMethod()]
        public void RunTest()
        {
            TaskProvider p=new TaskProvider();
            p.Run(3);
            Assert.Fail();
        }
    }
}