using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using NLog.LayoutRenderers;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.Resources;
using OneClickDesktop.BackendClasses.Model.States;
using OneClickDesktop.RabbitModule.Common.EventArgs;
using OneClickDesktop.VirtualizationLibrary.Vagrant;
using OneClickDesktop.VirtualizationServer.Messages;

namespace OneClickDesktop.VirtualizationServer
{
    /// <summary>
    /// Klasa zajmuje sie prztwarzaniem zapytaniań uzyskanych z kolejki
    /// </summary>
    public static class CommunicationLoop
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static RunningServices runningServices;

        //private static object modelLock;
        
        /// <summary>
        /// Konstruktor podłącza się do nasłuchiwania na kolejkach wejściowych do serwera.
        /// </summary>
        /// <remarks>Głowny wątek trzeba zablokwoac w inny sposób.</remarks>
        /// <remarks>
        /// Nie powinno sie uruchamiać wielu konstruktorów tej klasy -
        /// wtedy każde zapytanie zostanie porzetworzone tyle razy ile będzie wywołanych konstruktorów
        /// </remarks>
        /// <param name="services">Prawidłowo zainicjalizowany zbiór serviców</param>
        public static void RegisterReadingLogic(RunningServices services)
        {
            runningServices = services;
            runningServices.OverseersCommunication.RegisterReaderLoop(ConsumeOverseerRequests);
        }
        
        private static void ProcessDomainStartupRequest(DomainStartupRDTO request)
        {
            if (request == null)
            {
                logger.Warn($"DomainStartupMessage was broken - data deserialisation or conversion failed");
                return;
            }
            
            logger.Info($"Processing DomainStartupRequest {JsonSerializer.Serialize(request)}");

            var machine = runningServices.ModelManager.GetMachine(request.DomainName);
            if (machine != null && machine.State != MachineState.TurnedOff)
            {
                logger.Info($"Requesting startup of machine {request.DomainName} but it is already running");
                return;
            }

            TemplateResources resources = runningServices.ModelManager.GetTemplateResources(request.DomainType);
            if (resources == null)
            {
                logger.Warn($"Machine of type {request.DomainType} is not registered at this server. Skipping request");
                return;
            }

            if (!runningServices.VirtualizationManager
                .DomainStartup(request.DomainName, runningServices.ModelManager.GetTemplateResources(request.DomainType), out IPAddress address))
            {
                logger.Warn($"Startup of machine {request.DomainName}, type {request.DomainType}, failed");
                return;
            }
            
            runningServices.ModelManager.CreateRunningMachine(request.DomainName, request.DomainType, address);
            runningServices.OverseersCommunication.ReportModel(runningServices.ModelManager.GetReport());
        }
        
        private static void ProcessDomainShutdownRequest(DomainShutdownRDTO request)
        {
            if (request == null)
            {
                logger.Warn($"DomainShutdownMessage was broken - data deserialisation or conversion failed");
                return;
            }
            
            logger.Info($"Processing DomainShutdownRequest {JsonSerializer.Serialize(request)}");

            var machine = runningServices.ModelManager.GetMachine(request.DomainName);
            if (machine == null)
            {
                logger.Info($"Requesting shutdown of machine {request.DomainName} but it doesn't exist");
                return;
            }
            if (!Constants.State.MachineAvailableForShutdown.Contains(machine.State))
            {
                logger.Info($"Requesting shutdown of machine {request.DomainName} but it is not available for shutdown, actual state is {machine.State}");
                return;
            }
            
            if (!runningServices.VirtualizationManager.DomainShutdown(request.DomainName))
            {
                logger.Info($"Shutdown of machine {request.DomainName} failed");
                return;
            }
            
            runningServices.ModelManager.DeleteMachine(request.DomainName);
            runningServices.OverseersCommunication.ReportModel(runningServices.ModelManager.GetReport());
        }
        
        private static void ProcessSessionCreationRequest(SessionCreationRDTO request)
        {
            if (request == null)
            {
                logger.Warn($"SessionCreationMessage was broken - data deserialisation or conversion failed");
                return;
            }
            
            logger.Info($"Processing SessionCreationRequest {JsonSerializer.Serialize(request)}");
            var machine = runningServices.ModelManager.GetMachine(request.DomainName);
            if (machine == null)
            {
                logger.Info($"Requesting machine {request.DomainName} for session but it doesn't exist");
                return;
            }
            
            if (!Constants.State.MachineAvailableForSession.Contains(machine.State))
            {
                logger.Info($"Requesting machine {request.DomainName} for session but it is not available for session, actual state is {machine.State}");
                return;
            }
            
            if (machine.MachineType.Type != request.PartialSession.SessionType.Type)
            {
                logger.Info($"Requesting machine {request.DomainName} for session type {request.PartialSession.SessionType.Type} but it cannot handle it, machine type is {machine.MachineType}");
                return;
            }

            try
            {
                runningServices.ModelManager.CreateSession(request.PartialSession, request.DomainName);
            }
            catch (Exception e)
            {
                logger.Info(e.Message);
                return;
            }

            runningServices.OverseersCommunication.ReportModel(runningServices.ModelManager.GetReport());
        }

        private static void ProcessModelReportRequest()
        {
            logger.Info("Processing ModelReportRequest");
            runningServices.OverseersCommunication.ReportModel(runningServices.ModelManager.GetReport());
        }
        
        /// <summary>
        /// Metoda wywoływana w przypadku otrzymania wiadomości z kolejki wspólnej lub bezpośredniej.
        /// W aktualnym stanie systemu nie ma znaczenia skąd przyszła wiadomość. Można to łatwo zmienić modyfikując konstruktor.
        /// </summary>
        /// <remarks>Wywołuje sie na wątku nasłuchującym na kolejce rabbita!</remarks>
        /// <param name="sender">Wywołujący event</param>
        /// <param name="args">Dane otrzymane w kolejce</param>
        private static void ConsumeOverseerRequests(object sender, MessageEventArgs args)
        {
            logger.Debug($"Received message from {args.RabbitMessage.SenderIdentifier} of type {DomainStartupMessage.MessageTypeName}");
            switch (args.RabbitMessage.Type)
            {
                case DomainStartupMessage.MessageTypeName:
                    DomainStartupRDTO domainStartup = args.RabbitMessage.Body as DomainStartupRDTO;
                    ProcessDomainStartupRequest(domainStartup);
                    break;
                case DomainShutdownMessage.MessageTypeName:
                    DomainShutdownRDTO domainShutdown = args.RabbitMessage.Body as DomainShutdownRDTO;
                    ProcessDomainShutdownRequest(domainShutdown);
                    break;
                case SessionCreationMessage.MessageTypeName:
                    SessionCreationRDTO sessionCreation = args.RabbitMessage.Body as SessionCreationRDTO;
                    ProcessSessionCreationRequest(sessionCreation);
                    break;
                case ModelReportMessage.MessageTypeName:
                    ProcessModelReportRequest();
                    break;
                default:
                    logger.Warn("Message type doesn't recognised - refuse to process data");
                    break;
            }
        }
    }
}