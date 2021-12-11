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
    /// Klasa rperezentuje model uruchomionego servera. rejestruje sie w niej wszystkie zmienione sesje i maszyny.
    /// Na początku musi znac maksymalne dostępne zasoby.
    /// </summary>
    public class ModelManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        private BackendClasses.Model.VirtualizationServer model;

        public ModelManager(ServerResources totalResources,
            IDictionary<string, TemplateResources> templates)
        {
            logger.Info("Creating ModelManager");
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