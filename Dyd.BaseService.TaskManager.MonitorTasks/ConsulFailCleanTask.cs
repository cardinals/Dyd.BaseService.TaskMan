using System;
using Consul;

namespace Dyd.BaseService.TaskManager.MonitorTasks
{
    /// <summary>
    /// 对于无效的服务进行清除
    /// </summary>
    public class ConsulFailCleanTask : XXF.BaseService.TaskManager.BaseDllTask
    {
        public override void Run()
        {
            string serverList = AppConfig["servers"];
            string[] ls=serverList.Split(",".ToCharArray());
            foreach (var s in ls)
            {
                Clean(s);    
            }
            
        }

        private  void Clean(string serverUrl)
        {
            var client = new Consul.ConsulClient(configuration => { configuration.Address = new Uri(serverUrl); });

            var checks = client.Agent.Checks().Result.Response; // client.Agent.Services().Result.Response;
            foreach (var service in checks.Values)
            {
                if (service.Status != HealthStatus.Passing)
                {
                    // logger.info("unregister : {}", check.getServiceId());
                    Console.WriteLine($"unreginer:{service.ServiceID}");
                    OpenOperator.Log($"unreginer:{service.ServiceID}");
                    client.Agent.ServiceDeregister(service.ServiceID);
                }
            }
        }

        public override void TestRun()
        {
            this.AppConfig = new XXF.BaseService.TaskManager.SystemRuntime.TaskAppConfigInfo();
            this.AppConfig.Add("servers", "http://10.3.13.58:8011/");
          

            base.TestRun();
        }
    }

   
}