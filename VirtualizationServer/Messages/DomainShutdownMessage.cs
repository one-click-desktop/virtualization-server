using OneClickDesktop.BackendClasses.Communication.MessagesTemplates;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;

namespace OneClickDesktop.VirtualizationServer.Messages
{
    public class DomainShutdownMessage: DomainShutdownTemplate, IRabbitMessage
    {
        public string AppId { get; set; } = Configuration.AppId;
        public string Type { get; set; } = MessageTypeName;
        public object Message { get; set; }

        public DomainShutdownMessage(DomainShutdownRDTO data)
        {
            Message = data;
        }
    }
}