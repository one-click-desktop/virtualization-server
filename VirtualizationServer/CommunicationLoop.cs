using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NLog.LayoutRenderers;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.Resources;
using OneClickDesktop.BackendClasses.Model.States;
using OneClickDesktop.RabbitModule.Common.EventArgs;
using OneClickDesktop.VirtualizationLibrary.Vagrant;
using OneClickDesktop.VirtualizationServer.Configuration.ConfigurationClasses;
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

        private static object modelLock = new object();
        /// <summary>
        /// Timer liczy czas od ostatniej otrzymanej wiadomości.
        /// </summary>
        private static System.Timers.Timer receiveCommandTimer;
        
        /// <summary>
        /// Konstruktor podłącza się do nasłuchiwania na kolejkach wejściowych do serwera.
        /// </summary>
        /// <remarks>Głowny wątek trzeba zablokwoac w inny sposób.</remarks>
        /// <remarks>
        /// Nie powinno sie uruchamiać wielu konstruktorów tej klasy -
        /// wtedy każde zapytanie zostanie porzetworzone tyle razy ile będzie wywołanych konstruktorów
        /// </remarks>
        /// <param name="services">Prawidłowo zainicjalizowany zbiór serviców</param>
        /// <param name="exitSemaphore">Semafor, który po zwolnieniu wyłączy server.</param>
        public static void RegisterReadingLogic(VirtSrvConfiguration virtSrvConfig, RunningServices services, Semaphore exitSemaphore)
        {
            runningServices = services;
            runningServices.OverseersCommunication.RegisterReaderLoop(ConsumeOverseerRequests);
            
            //[TODO][CONFIG] Wynieść do configuracji
            receiveCommandTimer = new System.Timers.Timer(virtSrvConfig.OversserCommunicationShutdownTimeout * 1000);
            //Jeżeli timer się skończy => możliwe, że brakuje overseerów - zakończ prace servera
            receiveCommandTimer.Enabled = true;
            receiveCommandTimer.Elapsed += (obj, args) =>
            {
                logger.Info("Timeout on message from overseers - stopping server.");
                exitSemaphore.Release();
            };
            runningServices.OverseersCommunication.RegisterReaderLoop(RestartTimeoutTimer);

            runningServices.ClientHeartbeat.Missing += HandleMissing;
            runningServices.ClientHeartbeat.Found += HandleFound;
        }
        
        

        private static Action AsyncDomainStartup(DomainStartupRDTO request)
        {
            bool success = runningServices.VirtualizationManager
                .DomainStartup(request.DomainName,
                    runningServices.ModelManager.GetTemplateResources(request.DomainType), out IPAddress address);
            
            lock (modelLock)
            {
                if (!success)
                {
                    logger.Warn($"Startup of machine {request.DomainName}, type {request.DomainType}, failed");

                    //Usunięcie wystartowanej maszyny z błedem
                    runningServices.ModelManager.DeleteMachine(request.DomainName);
                    logger.Info($"Domain {request.DomainName} startup failed.");
                }
                else
                {
                    //Zmiana stanu prawidłowo wystartowanej maszyny
                    Machine m = runningServices.ModelManager.GetMachine(request.DomainName);
                    m.State = MachineState.Free;
                    m.AssignAddress(new MachineAddress(address.MapToIPv4().ToString()));
                    logger.Info($"Domain {request.DomainName} startup succeded.");
                }
                runningServices.OverseersCommunication.ReportModel(runningServices.ModelManager.GetReport());
            }

            return null;
        }
        
        #region Request processing
        private static void ProcessDomainStartupRequest(DomainStartupRDTO request)
        {
            if (request == null)
            {
                logger.Warn($"DomainStartupMessage was broken - data deserialisation or conversion failed");
                return;
            }
            
            logger.Info($"Processing DomainStartupRequest {JsonSerializer.Serialize(request)}");
            lock (modelLock)
            {
                var machine = runningServices.ModelManager.GetMachine(request.DomainName);
                if (machine != null && machine.State != MachineState.TurnedOff)
                {
                    logger.Info($"Requesting startup of machine {request.DomainName} but it is already running");
                    return;
                }

                TemplateResources resources = runningServices.ModelManager.GetTemplateResources(request.DomainType);
                if (resources == null)
                {
                    logger.Warn(
                        $"Machine of type {request.DomainType} is not registered at this server. Skipping request");
                    return;
                }
                
                runningServices.ModelManager.CreateBootingMachine(request.DomainName, request.DomainType);
                Task.Run(() => AsyncDomainStartup(request));

                runningServices.OverseersCommunication.ReportModel(runningServices.ModelManager.GetReport());
            }
        }
        
        private static void ProcessDomainShutdownRequest(DomainShutdownRDTO request)
        {
            if (request == null)
            {
                logger.Warn($"DomainShutdownMessage was broken - data deserialisation or conversion failed");
                return;
            }
            
            logger.Info($"Processing DomainShutdownRequest {JsonSerializer.Serialize(request)}");
            lock (modelLock)
            {
                var machine = runningServices.ModelManager.GetMachine(request.DomainName);
                if (machine == null)
                {
                    logger.Info($"Requesting shutdown of machine {request.DomainName} but it doesn't exist");
                    return;
                }

                if (!Constants.State.MachineAvailableForShutdown.Contains(machine.State))
                {
                    logger.Info(
                        $"Requesting shutdown of machine {request.DomainName} but it is not available for shutdown, actual state is {machine.State}");
                    return;
                }

                if (!runningServices.VirtualizationManager.DomainShutdown(request.DomainName))
                {
                    logger.Info($"Shutdown of machine {request.DomainName} failed");
                    return;
                }

                var session = runningServices.ModelManager.GetSessionForMachine(machine.Name);
                runningServices.ClientHeartbeat.RemoveQueue(session.SessionGuid.ToString());
                runningServices.ModelManager.DeleteSession(session.SessionGuid);

                runningServices.ModelManager.DeleteMachine(request.DomainName);
                runningServices.OverseersCommunication.ReportModel(runningServices.ModelManager.GetReport());
            }
        }
        
        private static void ProcessSessionCreationRequest(SessionCreationRDTO request)
        {
            if (request == null)
            {
                logger.Warn($"SessionCreationMessage was broken - data deserialisation or conversion failed");
                return;
            }
            
            logger.Info($"Processing SessionCreationRequest {JsonSerializer.Serialize(request)}");
            lock (modelLock)
            {
                var machine = runningServices.ModelManager.GetMachine(request.DomainName);
                if (machine == null)
                {
                    logger.Info($"Requesting machine {request.DomainName} for session but it doesn't exist");
                    return;
                }

                if (!Constants.State.MachineAvailableForSession.Contains(machine.State))
                {
                    logger.Info(
                        $"Requesting machine {request.DomainName} for session but it is not available for session, actual state is {machine.State}");
                    return;
                }

                if (machine.MachineType.Type != request.PartialSession.SessionType.Type)
                {
                    logger.Info(
                        $"Requesting machine {request.DomainName} for session type {request.PartialSession.SessionType.Type} but it cannot handle it, machine type is {machine.MachineType}");
                    return;
                }

                Session session;
                try
                {
                    session = runningServices.ModelManager.CreateSession(request.PartialSession, request.DomainName);
                    session.AttachMachine(machine);
                }
                catch (Exception e)
                {
                    logger.Info(e.Message);
                    return;
                }

                runningServices.ClientHeartbeat.RegisterQueue(session.SessionGuid.ToString());
                runningServices.OverseersCommunication.ReportModel(runningServices.ModelManager.GetReport());
            }
        }

        private static void ProcessModelReportRequest()
        {
            logger.Info("Processing ModelReportRequest");
            lock (modelLock)
            {
                runningServices.OverseersCommunication.ReportModel(runningServices.ModelManager.GetReport());
            }
        }
        #endregion

        #region ClientHeartbeat handler
        private static void HandleMissing(object sender, string queue)
        {
            if (!Guid.TryParse(queue, out var sessionGuid))
            {
                logger.Warn("Cannot parse queue name to session guid");
            }

            var session = runningServices.ModelManager.GetSession(sessionGuid);

            if (session == null)
            {
                logger.Info($"Session not found for guid: {sessionGuid}");
                return;
            }

            session.SessionState = SessionState.WaitingForRemoval;
            runningServices.OverseersCommunication.ReportModel(runningServices.ModelManager.GetReport());
        }
        
        private static void HandleFound(object sender, string queue)
        {
            if (!Guid.TryParse(queue, out var sessionGuid))
            {
                logger.Warn("Cannot parse queue name to session guid");
            }

            var session = runningServices.ModelManager.GetSession(sessionGuid);

            if (session == null)
            {
                logger.Info($"Session not found for guid: {sessionGuid}");
                return;
            }

            session.SessionState = SessionState.Running;
            runningServices.OverseersCommunication.ReportModel(runningServices.ModelManager.GetReport());
        }
        #endregion
        
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
                case PingMessage.MessageTypeName:
                    logger.Debug("Ping message - ignore");
                    break;
                default:
                    logger.Warn("Message type doesn't recognised - refuse to process data");
                    break;
            }
        }

        private static void RestartTimeoutTimer(object sender, MessageEventArgs args)
        {
            //Tutaj teoretycznie timer może się skończyć pomiędzy Enabled = false a stopem
            //Jednak my się tym nie przejmujemy - w sensie taki błąd jest dla nas akceptowalny.
            //liczymy sekundy a nie milisekundy
            receiveCommandTimer.Enabled = false;
            receiveCommandTimer.Stop();//reset
            receiveCommandTimer.Start();//reset
            receiveCommandTimer.Enabled = true;
        }
    }
}