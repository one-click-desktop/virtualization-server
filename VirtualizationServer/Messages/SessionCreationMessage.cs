using OneClickDesktop.BackendClasses.Communication.MessagesTemplates;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;

namespace OneClickDesktop.VirtualizationServer.Messages
{
    public class SessionCreationMessage: SessionCreationTemplate, IRabbitMessage
    {
        public string SenderIdentifier { get; set; }
        public string Type { get; set; } = MessageTypeName;
        public object Body { get; set; }

        public SessionCreationMessage(SessionCreationRDTO data)
        {
            Body = data;
        }
    }
}