using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using NLog;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.Resources;
using OneClickDesktop.BackendClasses.Model.States;
using OneClickDesktop.VirtualizationServer.Messages;

namespace OneClickDesktop.VirtualizationServer.Services
{
    /// <summary>
    /// Klasa reprezentuje model uruchomionego servera. Rejestruje sie w niej wszystkie zmienione sesje i maszyny.
    /// Na początku musi znac maksymalne dostępne zasoby oraz wzorce zasobó dla każdego typu maszyny.
    /// </summary>
    public class ModelManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        private BackendClasses.Model.VirtualizationServer model;

        public ModelManager(string directQueueName, ServerResources totalResources,
                            IDictionary<string, TemplateResources> templates)
        {
            logger.Info("Creating ModelManager");
            model = new BackendClasses.Model.VirtualizationServer(totalResources, templates, directQueueName);
            logger.Info($"Server managed resources: {JsonSerializer.Serialize(totalResources)}");
            logger.Info($"Loaded machine templates: {String.Join(",\n", templates.Select(t => $"{t.Key}:{JsonSerializer.Serialize(t.Value)}"))}");
        }

        public ModelReportMessage GetReport()
        {
            return new ModelReportMessage(model);
        }

        public Machine GetMachine(string name)
        {
            return model.RunningMachines.TryGetValue(name, out var machine) ? machine : null;
        }

        public Session CreateSession(Session partialSession, string machineName)
        {
            return model.CreateFullSession(partialSession, machineName);
        }

        public void DeleteMachine(string machineName)
        {
            model.DeleteMachine(machineName);
        }

        public void CreateRunningMachine(string domainName, MachineType type, IPAddress addr)
        {
            Machine m = model.CreateMachine(domainName, type);
            
            m.State = MachineState.Free;
            m.AssignAddress(new MachineAddress(addr.MapToIPv4().ToString()));
        }
        
        public void CreateBootingMachine(string domainName, MachineType type, IPAddress addr)
        {
            Machine m = model.CreateMachine(domainName, type);
            
            m.State = MachineState.Booting;
            m.AssignAddress(new MachineAddress(addr.MapToIPv4().ToString()));
        }

        public TemplateResources GetTemplateResources(MachineType type)
        {
            if (!model.TemplateResources.TryGetValue(type.Type, out TemplateResources res))
                return null;
            return res;
        }
    }
}