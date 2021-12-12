using System;
using System.Collections.Generic;
using NLog;
using OneClickDesktop.BackendClasses.Model.Resources;
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
        
        private static VirtualizationManager PrepareVirtualizationManager()
        {
            string libvirtUri = "qemu:///system";//[TODO][CONFIG] Wynieść do konfiguracji!
            string vagrantFile = "res/Vagrantfile";//[TODO][CONFIG] Wynieść do konfiguracji!
            return new VirtualizationManager(libvirtUri, vagrantFile);
        }
        
        private static ModelManager PrepareModelManager(string directQueueName)
        {
            ServerResources totalResources = new ServerResources(4096, 4, 200, new List<GpuId>());//[TODO][CONFIG] Wynieść do konfiguracji!
            Dictionary<string, TemplateResources> templates = new Dictionary<string, TemplateResources>();
            templates["cpu"] = new TemplateResources(2048, 4, 20, false);//[TODO][CONFIG] Wynieść do konfiguracji!
            
            return new ModelManager(directQueueName, totalResources, templates);
        }

        private static OverseersCommunication PrepareOverseersCommunication()
        {
            OverseersCommunicationParameters parameters = new OverseersCommunicationParameters()
            {
                RabbitMQHostname = "localhost", //[TODO][CONFIG] Wynieść do konfiguracji!
                RabbitMQPort = 5672, //[TODO][CONFIG] Wynieść do konfiguracji!
                MessageTypeMappings = new Dictionary<string, Type>()
            };
            
            return new OverseersCommunication(parameters);
        }

        public static RunningServices InitializeVirtualizationServer()
        {
            logger.Info("Initializing Virtualization Server");

            RunningServices res = new RunningServices();
            res.VirtualizationManager = PrepareVirtualizationManager();
            res.OverseersCommunication = PrepareOverseersCommunication();
            res.ModelManager = PrepareModelManager(res.OverseersCommunication.DirectQueueName);
            
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