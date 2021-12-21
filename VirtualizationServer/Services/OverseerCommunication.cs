using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using OneClickDesktop.RabbitModule.Common.EventArgs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;
using OneClickDesktop.RabbitModule.VirtualizationServer;
using OneClickDesktop.VirtualizationServer.Messages;

namespace OneClickDesktop.VirtualizationServer.Services
{
    public class OverseersCommunicationParameters
    {
        public string RabbitMQHostname;
        public int RabbitMQPort;
        public IReadOnlyDictionary<string, Type> MessageTypeMappings;
        public string VirtSrvId;
    }
    
    public class OverseersCommunication: IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private VirtualizationServerClient connection;
        private string appId;
        
        public string DirectQueueName => connection.DirectQueueName;

        public OverseersCommunication(OverseersCommunicationParameters parameters)
        {
            logger.Info("Creating OverseersCommunication");
            connection = new VirtualizationServerClient(parameters.RabbitMQHostname, parameters.RabbitMQPort, parameters.MessageTypeMappings);
            appId = parameters.VirtSrvId;
        }
        
        public void RegisterReaderLoop(EventHandler<MessageEventArgs> reader)
        {
            connection.CommonReceived += reader;
            connection.DirectReceived += reader;
        }
        
        /// <summary>
        /// Signs rabbit message with system-wide unique application identifier
        /// </summary>
        /// <param name="msg">Message to sign</param>
        /// <returns>Signed message</returns>
        private IRabbitMessage SignRabbitPackage(IRabbitMessage msg)
        {
            msg.AppId = appId;
            return msg;
        }

        /// <summary>
        /// Reports model to overseers
        /// </summary>
        /// <param name="model">Model to report</param>
        public void ReportModel(ModelReportMessage model)
        {
            connection.SendToOverseers(SignRabbitPackage(model));
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

        private void InitializationReturnHandler(object model, ReturnEventArgs args)
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