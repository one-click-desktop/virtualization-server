using OneClickDesktop.BackendClasses.Communication.MessagesTemplates;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;

namespace OneClickDesktop.VirtualizationServer.Messages
{
    public class DomainStartupMessage: DomainStartupTemplate, IRabbitMessage
    {
        public string AppId { get; set; }
        public string Type { get; set; } = MessageTypeName;
        public object Message { get; set; }

        public DomainStartupMessage(DomainStartupRDTO data)
        {
            Message = data;
        }
    }
}