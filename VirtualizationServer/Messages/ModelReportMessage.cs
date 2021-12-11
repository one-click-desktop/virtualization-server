using System.Text.Json;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;
using OneClickDesktop.BackendClasses.Model;

namespace OneClickDesktop.VirtualizationServer.Messages
{
    public class ModelReportMessage: IRabbitMessage
    {
        public string AppId { get; set; } = "NaRazieTestowoUstawmyCosTakiego";
        public string Type { get; set; } = "ModelReport";
        public object Message { get; set; }


        public ModelReportMessage(BackendClasses.Model.VirtualizationServer model)
        {
            Message = model;
        }
    }
}