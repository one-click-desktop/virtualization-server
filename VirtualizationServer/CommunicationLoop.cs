using System.Runtime.CompilerServices;
using System.Text.Json;
using NLog.LayoutRenderers;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.RabbitModule.Common.EventArgs;
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
            
            //Tutaj póżniej normalne przetwarzanie danych
            logger.Info($"Processing DomainStartupRequest {JsonSerializer.Serialize(request)}");
        }
        
        private static void ProcessDomainShutdownRequest(DomainShutdownRDTO request)
        {
            if (request == null)
            {
                logger.Warn($"DomainShutdownMessage was broken - data deserialisation or conversion failed");
                return;
            }
            
            //Tutaj póżniej normalne przetwarzanie danych
            logger.Info($"Processing DomainShutdownRequest {JsonSerializer.Serialize(request)}");
        }
        
        private static void ProcessSessionCreationRequest(SessionCreationRDTO request)
        {
            if (request == null)
            {
                logger.Warn($"SessionCreationMessage was broken - data deserialisation or conversion failed");
                return;
            }
            
            //Tutaj póżniej normalne przetwarzanie danych
            logger.Info($"Processing SessionCreationRequest {JsonSerializer.Serialize(request)}");
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
            logger.Debug($"Received message from {args.RabbitMessage.AppId} of type {DomainStartupMessage.MessageTypeName}");
            switch (args.RabbitMessage.Type)
            {
                case DomainStartupMessage.MessageTypeName:
                    DomainStartupRDTO domainStartup = DomainStartupMessage.ConversionReceivedData(args.RabbitMessage.Type);
                    ProcessDomainStartupRequest(domainStartup);
                    break;
                case DomainShutdownMessage.MessageTypeName:
                    DomainShutdownRDTO domainShutdown = DomainShutdownMessage.ConversionReceivedData(args.RabbitMessage.Type);
                    ProcessDomainShutdownRequest(domainShutdown);
                    break;
                case SessionCreationMessage.MessageTypeName:
                    SessionCreationRDTO sessionCreation = SessionCreationMessage.ConversionReceivedData(args.RabbitMessage.Type);
                    ProcessSessionCreationRequest(sessionCreation);
                    break;
                default:
                    logger.Warn("Message type doesn't recognised - refuse to process data");
                    break;
            }
        }
    }
}