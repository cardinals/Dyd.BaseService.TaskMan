using System;
using System.Data;
using System.IO;
using Consul;
using Dyd.BaseService.TaskManager.Domain.Model;
using ServiceStack.Text;
using XXF.BaseService.TaskManager.SystemRuntime;

namespace Dyd.BaseService.TaskManager.Node.SystemRuntime
{
    public class ConsulRegisterManger
    {
        
        public  ConsulRegisteration Parse(tb_task_model taskruntimeinfoTaskModel)
        {
            ConsulRegisteration item=new ConsulRegisteration();
            TaskAppConfigInfo config = taskruntimeinfoTaskModel
                .taskappconfigjson.FromJson<TaskAppConfigInfo>();

            Uri service = new Uri(config["service_url"]);
            item.Host = service.Host;
            item.Port = service.Port;

            string serviceNames = Path.GetFileNameWithoutExtension(taskruntimeinfoTaskModel.taskmainclassdllfilename);
            item.Service = serviceNames;
            item.ServiceId = $"{serviceNames}_{item.Host}_{item.Port}";
            return item;
        }

        public async void Register(ConsulRegisteration item)
        {
            var client = new ConsulClient(configuration =>
                            {
                                configuration.Address=new Uri(GlobalConfig.Consule);
                            }); // uses default host:port which is localhost:8500
                            //SpinWait.SpinUntil(() => (taskruntimeinfo.Process.MainWindowHandle != IntPtr.Zero));
                            //while (taskruntimeinfo.Process.MainWindowHandle == IntPtr.Zero)
                              //  Application.DoEvents();
                            //修改title
                            //WinApi.SetWindowText(taskruntimeinfo.Process.Handle, item.ServiceId);
            
            
                             var agentReg = new AgentServiceRegistration()
                            {
                                Address = item.Host,
                                ID =item.ServiceId,
                                Name =item.Service,
                                Port =item.Port
                            };

            await client.Agent.ServiceRegister(agentReg);
            var reg = new AgentCheckRegistration()
            {
                Name = item.Service,
                ServiceID = item.ServiceId,
                TTL = TimeSpan.FromSeconds(15)
            };
            await client.Agent.CheckRegister(reg);
 
          
        }

        public async void UnRegister(ConsulRegisteration item)
        {
             var client = new ConsulClient(configuration =>
                               {
                                   configuration.Address = new Uri(GlobalConfig.Consule);
                               }); // uses default host:port which is localhost:8500
                               string service = item.ServiceId;
            await client.Agent.ServiceDeregister(service);
            await  client.Agent.CheckDeregister(item.ServiceId);
        }
    }
}