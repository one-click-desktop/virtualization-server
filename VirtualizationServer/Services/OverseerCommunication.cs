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
    
    /// <summary>
    /// Exception thrown when there is lack of overseers on the other side
    /// </summary>
    public class OverseerCommunicationException : Exception
    {
        public OverseerCommunicationException() { }
        public OverseerCommunicationException(string? message) : base(message) { }
        public OverseerCommunicationException(string? message, Exception? innerException) : base(message, innerException) { }
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
            connection.Return += InitializationReturnHandler;
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
            msg.SenderIdentifier = appId;
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

        #region Event handlers

        private void InitializationReturnHandler(object model, ReturnEventArgs args)
        {
            args.ReturnReason.
            throw new OverseerCommunicationException(args.ReplyText);
        }
        #endregion

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}