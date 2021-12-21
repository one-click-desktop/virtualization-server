using System;
using System.Collections.Generic;
using NLog;
using OneClickDesktop.BackendClasses.Communication;
using OneClickDesktop.BackendClasses.Model.Resources;
using OneClickDesktop.VirtualizationServer.Configuration.ConfigurationClasses;
using OneClickDesktop.VirtualizationServer.Services;

namespace OneClickDesktop.VirtualizationServer
{
    public class RunningServices: IDisposable
    {
        public VirtualizationManager VirtualizationManager;
        public OverseersCommunication OverseersCommunication;
        public ModelManager ModelManager;
        
        public void Dispose()
        {
            VirtualizationManager?.Dispose();
            OverseersCommunication?.Dispose();
        }
    }
    
    public static class StartProcedure
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        private static VirtualizationManager PrepareVirtualizationManager(VirtSrvConfiguration systemConfig)
        {
            return new VirtualizationManager(systemConfig);
        }
        
        private static ModelManager PrepareModelManager(string directQueueName, ResourcesConfiguration resourcesConfig)
        {
            ServerResources totalResources = new ServerResources(4096, 4, 200, new List<GpuId>());//[TODO][CONFIG] Wynieść do konfiguracji!
            Dictionary<string, TemplateResources> templates = new Dictionary<string, TemplateResources>();
            templates["cpu"] = new TemplateResources(2048, 4, 20, false);//[TODO][CONFIG] Wynieść do konfiguracji!
            
            return new ModelManager(directQueueName, totalResources, templates);
        }

        private static OverseersCommunication PrepareOverseersCommunication(VirtSrvConfiguration systemConfig)
        {
            OverseersCommunicationParameters parameters = new OverseersCommunicationParameters()
            {
                RabbitMQHostname = systemConfig.InternalRabbitMQHostname,
                RabbitMQPort = systemConfig.InternalRabbitMQPort,
                MessageTypeMappings = TypeMappings.VirtualizationServerReceiveMapping,
                VirtSrvId = systemConfig.VirtualizationServerId
            };
            
            return new OverseersCommunication(parameters);
        }

        public static RunningServices InitializeVirtualizationServer(VirtSrvConfiguration systemConfig, ResourcesConfiguration resourcesConfig)
        {
            logger.Info("Initializing Virtualization Server");

            // TODO: dodać serwis odpowiedzialny za client heartbeata
            RunningServices res = new RunningServices();
            res.VirtualizationManager = PrepareVirtualizationManager(systemConfig);
            res.OverseersCommunication = PrepareOverseersCommunication(systemConfig);
            res.ModelManager = PrepareModelManager(res.OverseersCommunication.DirectQueueName, resourcesConfig);
            
            //Spróbuj wysłać model do overseera
            //W przypadku niepowodzenia serwer nie może podjąć pracy
            logger.Info("First time brodcast model to overseers");
            try
            {
                res.OverseersCommunication.FirstReportModel(res.ModelManager.GetReport());
            }
            catch (Exception e)
            {
                res.Dispose();
                logger.Fatal(e, "No overseer has been found. Server cannot operate.");
                throw;
            }

            logger.Info("Virtualization successfuly initialized");
            return res;
        }
    }
}