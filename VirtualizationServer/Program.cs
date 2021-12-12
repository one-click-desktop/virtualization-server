using System;
using System.Threading;
using System.Threading.Tasks;
using OneClickDesktop.VirtualizationServer.Services;

namespace OneClickDesktop.VirtualizationServer
{
    class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static RunningServices services;

        private static Semaphore exitSemaphore;

        public static void Main()
        {
            try
            {
                //Wystartuj wszystkie potrzebne servicy
                services = StartProcedure.InitializeVirtualizationServer();
                
                //Zarejestruj logikę prztwarzania wiadomości
                CommunicationLoop.RegisterReadingLogic(services);

                //Oczekuj na SIGINT
                exitSemaphore = new Semaphore(0, 1);
                Console.CancelKeyPress += (sender, args) =>
                {
                    args.Cancel = true;
                    exitSemaphore.Release();
                };
                exitSemaphore.WaitOne();
                logger.Info("SIGINT received - shuting down server");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unhandled Expetion - server is going down");
            }
            finally
            {
                services?.Dispose();
                logger.Info("Server has gracefully stopped");
                NLog.LogManager.Shutdown();
            }
        }
    }
}