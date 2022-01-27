using System;
using OneClickDesktop.BackendClasses.Communication;
using OneClickDesktop.VirtualizationServer.Configuration;
using OneClickDesktop.VirtualizationServer.Services;
using OneClickDesktop.RabbitModule.Common.Exceptions;
using OneClickDesktop.RabbitModule.VirtualizationServer;

namespace OneClickDesktop.VirtualizationServer
{
    public class RunningServices: IDisposable
    {
        public VirtualizationManager VirtualizationManager;
        public OverseersCommunication OverseersCommunication;
        public HeartbeatClient ClientHeartbeat;
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
        
        private static VirtualizationManager PrepareVirtualizationManager(VirtSrvConfiguration systemConfig, NfsConfiguration nsfConf, LdapConfiguration ldapConf)
        {
            return new VirtualizationManager(systemConfig, nsfConf, ldapConf);
        }
        
        private static ModelManager PrepareModelManager(string directQueueName, ResourcesConfiguration resourcesConfig)
        {
            return new ModelManager(directQueueName,
                resourcesConfig.GetServerResources(), resourcesConfig.GetTemplateResources());
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

        private static HeartbeatClient PrepareClientHeartbeat(VirtSrvConfiguration systemConfig)
        {
            return new HeartbeatClient(systemConfig.ExternalRabbitMQHostname,
                systemConfig.ExternalRabbitMQPort,
                systemConfig.ClientHeartbeatChecksDelay, 
                systemConfig.ClientHeartbeatChecksForMissing);
        }

        public static RunningServices InitializeVirtualizationServer(ConfigurationCollection confCollection)
        {
            logger.Info("Initializing Virtualization Server");
            
            RunningServices res = new RunningServices();
            try
            {
                res.VirtualizationManager = PrepareVirtualizationManager(confCollection.VirtSrvConfiguration, confCollection.NfsConfiguration, confCollection.LdapConfiguration);
                res.OverseersCommunication = PrepareOverseersCommunication(confCollection.VirtSrvConfiguration);
                res.ClientHeartbeat = PrepareClientHeartbeat(confCollection.VirtSrvConfiguration);
                res.ModelManager = PrepareModelManager(res.OverseersCommunication.DirectQueueName, confCollection.ResourceConfiguration);

                logger.Info("First time brodcast model to overseers");
                res.OverseersCommunication.ReportModel(res.ModelManager.GetReport());
            }
            catch (BrokerConnectionException e)//Przypadek błednej komunikacji z brokerem
            {
                //TODO: brak podanego adresu dla ktorego wystapil blad (internal czy external rabbit?)
                res.Dispose();
                logger.Fatal("Cannot connect with broker. Server cannot operate. Is there any broker working over given address?");
                throw;
            }
            catch (MissingExchangeException e)//Przypadek niezadeklarowanego exchange
            {
                res.Dispose();
                logger.Fatal("Exchange is missing in broker. Server cannot operate. Is there any overseer working?");
                throw;
            }
            catch (OverseerCommunicationException e)//Przypadek powracającego returna przy wysłaniu modelu
            {
                res.Dispose();
                logger.Fatal("No overseer has been found. Server cannot operate.");
                throw;
            }

            logger.Info("Virtualization successfuly initialized");
            return res;
        }
    }
}