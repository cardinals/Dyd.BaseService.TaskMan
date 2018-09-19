using System;
using Consul;
using Dyd.BaseService.TaskManager.MonitorTasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dyd.BaseService.TaskManger.MonitorTaskTests
{
    [TestClass]
    public class FailCheckTests
    {
        [TestMethod]
        public void Test1()
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

        [TestMethod]
        public void test2()

        {
            ConsulFailCleanTask task=new ConsulFailCleanTask();
            task.TestRun();
        }
    }
}