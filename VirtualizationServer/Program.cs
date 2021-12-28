using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OneClickDesktop.VirtualizationServer.Configuration.ConfigurationClasses;
using OneClickDesktop.VirtualizationServer.Configuration.ConfigurationParsers;

namespace OneClickDesktop.VirtualizationServer
{
    class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static RunningServices services;

        private static Semaphore exitSemaphore;

        private static (VirtSrvConfiguration systemConfig, ResourcesConfiguration resourcesConfig) ParseConfiguration()
        {
            
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddIniFile("virtsrv.ini")//[TODO][ARGS] Wynieść ścieżke do parametrów programu(default: virtsrv.ini)
                //.AddIniFile("resources.ini")//[TODO][ARGS] Wynieść ścieżke do parametrów programu(default: resources.ini)
                .Build();


            var section = config.GetSection("OneClickDesktop");
            VirtSrvConfiguration systemConfig = section.Get<VirtSrvConfiguration>();

            return (systemConfig, null);
        }

        public static void Main()
        {
            try
            {
                //Wczytaj plik konfiguracyjny
                (VirtSrvConfiguration systemConfig, ResourcesConfiguration resourcesConfig) = ParseConfiguration();

                //Wystartuj wszystkie potrzebne servicy
                services = StartProcedure.InitializeVirtualizationServer(systemConfig, resourcesConfig);

                if (services != null)
                {

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