using Dyd.BaseService.TaskManager.Node.SystemRuntime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dyd.BaseService.TaskManager.NodeTests.SystemRuntime
{
    [TestClass]
    public class Testa

    {
        public Testa()
        {
        }

        [TestMethod]
        public void aa()

        {
            ConsulRegisterManger mgr = new ConsulRegisterManger();
            mgr.Register(new ConsulRegisteration {
                Host= "10.3.13.64",
                Service="aa",
                ServiceId=$"aa_localhost_88",
                Port=5071


            }, "http://10.1.13.56:8011");
        }
    }
}