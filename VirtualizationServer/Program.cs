﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OneClickDesktop.VirtualizationServer.Configuration;

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
                .AddIniFile("config/virtsrv.ini")//[TODO][ARGS] Wynieść ścieżke do parametrów programu(default: config/virtsrv.ini)
                .Build();


            var virtSrvSection = config.GetSection("OneClickDesktop");
            VirtSrvConfiguration systemConfig = virtSrvSection.Get<VirtSrvConfiguration>();
            var resourcesHeaderSection = config.GetSection("ServerResources");
            ResourcesHeaderConfiguration resourcesHeader = resourcesHeaderSection.Get<ResourcesHeaderConfiguration>();
            ResourcesConfiguration resourcesConfig = new ResourcesConfiguration(resourcesHeader, Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config"));//[TODO][ARGS] Wynieść ścieżke do parametrów programu(default: config/virtsrv.ini)
            
            return (systemConfig, resourcesConfig);
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
                    //Semafor mówiący czy trzeba zatrzymac server
                    exitSemaphore = new Semaphore(0, 1);
                    //Zarejestruj logikę prztwarzania wiadomości
                    CommunicationLoop.RegisterReadingLogic(systemConfig, services, exitSemaphore);

                    //Oczekuj na SIGINT
                    Console.CancelKeyPress += (sender, args) =>
                    {
                        args.Cancel = true;
                        logger.Info("SIGINT received - shuting down server");
                        exitSemaphore.Release();
                    };

                    exitSemaphore.WaitOne();
                    
                    //cleanup machines on gentle shutdown
                    services.VirtualizationManager.DomainCleanupOnShutdown(services.ModelManager.GetMachineNames());
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