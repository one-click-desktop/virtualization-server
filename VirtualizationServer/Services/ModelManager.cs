using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NLog;
using OneClickDesktop.BackendClasses.Model.Resources;
using OneClickDesktop.VirtualizationServer.Messages;

namespace OneClickDesktop.VirtualizationServer.Services
{
    /// <summary>
    /// Klasa reprezentuje model uruchomionego servera. Rejestruje sie w niej wszystkie zmienione sesje i maszyny.
    /// Na początku musi znac maksymalne dostępne zasoby oraz wzorce zasobó dla każdego typu maszyny.
    /// </summary>
    public class ModelManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        private BackendClasses.Model.VirtualizationServer model;
        private string directQueueName;

        public ModelManager(string directQueueName, ServerResources totalResources,
            IDictionary<string, TemplateResources> templates)
        {
            logger.Info("Creating ModelManager");
            this.directQueueName = directQueueName;
            model = new BackendClasses.Model.VirtualizationServer(totalResources, templates, "");//TODO: wyrzucic nazwe kolejki po zmianie wersji
            logger.Info($"Server managed resources: {JsonSerializer.Serialize(totalResources)}");
            logger.Info($"Loaded machine templates: {String.Join(",\n", templates.Select(t => $"{t.Key}:{JsonSerializer.Serialize(t.Value)}"))}");
        }

        public ModelReportMessage GetReport()
        {
            return new ModelReportMessage(model);
        }
    }
}