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

        public IEnumerable<string> GetMachineNames()
        {
            return model.RunningMachines.Values.Select(m => m.Name);
        }

        public Session CreateSession(Session partialSession, string machineName)
        {
            return model.CreateFullSession(partialSession, machineName);
        }

        public void DeleteMachine(string machineName)
        {
            model.DeleteMachine(machineName);
        }
        
        /// <summary>
        /// Create machine in booting state. Every modification to machine is made by reference.
        /// </summary>
        /// <param name="domainName"></param>
        /// <param name="type"></param>
        public void CreateBootingMachine(string domainName, MachineType type)
        {
            Machine m = model.CreateMachine(domainName, type);

            m.State = MachineState.Booting;
        }

        public TemplateResources GetTemplateResources(MachineType type)
        {
            if (!model.TemplateResources.TryGetValue(type.TechnicalName, out TemplateResources res))
                return null;
            return res;
        }

        public Session GetSession(Guid sessionGuid)
        {
            return model.Sessions.TryGetValue(sessionGuid, out var session) ? session : null;
        }

        public Session GetSessionForMachine(string machineName)
        {
            return model.Sessions.Values.FirstOrDefault(session => machineName.Equals(session.CorrelatedMachine?.Name));
        }

        public void DeleteSession(Guid sessionGuid)
        {
            model.DeleteSession(sessionGuid);
        }

        public bool CanServerRunMachine(TemplateResources template)
        {
            var res = model.FreeResources - template;
            return !(res.Memory < 0 || res.CpuCores < 0 || res.Storage < 0 || (template.AttachGpu && model.FreeResources.GpuCount < 1));
        }

        public bool HasRunningSessions()
        {
            return model.Sessions.Values.Any(session => session.SessionState == SessionState.Running);
        }
    }
}