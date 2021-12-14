using System;
using System.Collections.Generic;
using System.Runtime.Loader;
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

        private VirtualizationServerClient connection;
        
        public string DirectQueueName => connection.DirectQueueName;

        public OverseersCommunication(OverseersCommunicationParameters parameters)
        {
            logger.Info("Creating OverseersCommunication");
            connection = new VirtualizationServerClient(parameters.RabbitMQHostname, parameters.RabbitMQPort, parameters.MessageTypeMappings);
        }
        
        public void RegisterReaderLoop(EventHandler<MessageEventArgs> reader)
        {
            connection.CommonReceived += reader;
            connection.DirectReceived += reader;
        }

        public void ReportModel(ModelReportMessage model)
        {
            connection.SendToOverseers(model);
        }

        public void FirstReportModel(ModelReportMessage model)
        {
            connection.Return += InitializationReturnHandler;
            
            ReportModel(model);
            // to może nie zadziałać jeżeli return zostanie zwrócony z opóźnieniem
            // TODO: dodać returnHandler, który będzie informował że nie ma overseerów
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