using System;
using System.Collections.Generic;
using OneClickDesktop.RabbitModule.Common.EventArgs;
using OneClickDesktop.RabbitModule.VirtualizationServer;
using OneClickDesktop.VirtualizationServer.Messages;

namespace OneClickDesktop.VirtualizationServer.Services
{
    public class OverseersCommunicationParameters
    {
        public string RabbitMQHostname;
        public int RabbitMQPort;
        public IReadOnlyDictionary<string, Type> MessageTypeMappings;
    }
    
    public class OverseersCommunication: IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private RabbitModule.VirtualizationServer.VirtualizationServerClient connection;
        
        public string DirectQueueName
        {
            get => connection.DirectQueueName;
        } 

        public OverseersCommunication(OverseersCommunicationParameters parameters)
        {
            logger.Info("Creating OverseersCommunication");
            connection = new VirtualizationServerClient(parameters.RabbitMQHostname, parameters.RabbitMQPort, parameters.MessageTypeMappings);
        }

        public void ReportModel(ModelReportMessage model)
        {
            connection.SendToOverseers(model);
        }

        public void FirstReportModel(ModelReportMessage model)
        {
            connection.Return += InitializationReturnHandler;
            
            ReportModel(model);

            connection.Return -= InitializationReturnHandler;
        }
        
        #region Event handlers

        private void InitializationReturnHandler(object? model, ReturnEventArgs args)
        {
            throw new Exception(args.ReplyText);//[TODO] Zmienić typ wyjątku na jakis konkretniejszy
        }
        #endregion

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}