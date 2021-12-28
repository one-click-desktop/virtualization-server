using System;
using System.Drawing.Imaging;
using System.Text.Json;
using OneClickDesktop.BackendClasses.Communication.MessagesTemplates;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;
using OneClickDesktop.BackendClasses.Model;

namespace OneClickDesktop.VirtualizationServer.Messages
{
    public class ModelReportMessage: ModelReportTemplate, IRabbitMessage
    {
        public string AppId { get; set; }
        public string Type { get; set; } = MessageTypeName;
        public object Message { get; set; }

        public ModelReportMessage(BackendClasses.Model.VirtualizationServer model)
        {
            Message = model;
        }
    }
}