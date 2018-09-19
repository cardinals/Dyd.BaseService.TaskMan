using System;
using System.Threading.Tasks;
using Consul;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dyd.BaseService.TaskManager.MonitorTasksTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            var client = new ConsulClient(configuration =>
            {
                configuration.Address = new Uri("http://10.3.13.58:8011/");
            });

            var checks = client.Agent.Checks().Result.Response; // client.Agent.Services().Result.Response;
            foreach (var service in checks.Values)
            {
                if (service.Status != HealthStatus.Passing)
                {
                    // logger.info("unregister : {}", check.getServiceId());
                    Console.WriteLine($"unreginer:{service.ServiceID}");
                    client.Agent.ServiceDeregister(service.ServiceID);
                }
            }
        }



    }
}
