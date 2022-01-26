using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Configuration;
using NLog.Fluent;
using OneClickDesktop.VirtualizationServer.Configuration;

namespace OneClickDesktop.VirtualizationServer
{
    class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static RunningServices services;

        private static Semaphore exitSemaphore;

        private static ConfigurationCollection ParseConfiguration(string configFolderPath)
        {
            
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddIniFile(Path.Join(configFolderPath, "virtsrv.ini"))
                .Build();
            
            VirtSrvConfiguration systemConfig = config.GetSection("OneClickDesktop").Get<VirtSrvConfiguration>() ?? new VirtSrvConfiguration();
            NfsConfiguration nfsConf = config.GetSection("Nfs").Get<NfsConfiguration>() ?? new NfsConfiguration();
            LdapConfiguration ldapConf = config.GetSection("Ldap").Get<LdapConfiguration>() ?? new LdapConfiguration();
            
            ResourcesConfiguration resourcesConfig = new ResourcesConfiguration(config, configFolderPath);

            return new ConfigurationCollection()
            {
                ResourceConfiguration = resourcesConfig,
                VirtSrvConfiguration = systemConfig,
                NfsConfiguration = nfsConf,
                LdapConfiguration = ldapConf
            };
        }

        static void Main(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<Options>(args);
            parseResult.WithParsed(RunOptions);
            parseResult.WithNotParsed(errs => HandleParseError(parseResult, errs));
        }
        static void RunOptions(Options opts)
        {
            try
            {
                //Wczytaj plik konfiguracyjny
                ConfigurationCollection configs = ParseConfiguration(Path.GetFullPath(opts.ConfigurationFolderPath));

                //Wystartuj wszystkie potrzebne servicy
                services = StartProcedure.InitializeVirtualizationServer(configs);

                if (services != null)
                {
                    //Semafor mówiący czy trzeba zatrzymac server
                    exitSemaphore = new Semaphore(0, 1);
                    //Zarejestruj logikę prztwarzania wiadomości
                    CommunicationLoop.RegisterReadingLogic(configs.VirtSrvConfiguration, services, exitSemaphore);

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
        static void HandleParseError<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var builder = SentenceBuilder.Create();
            var errorMessages = HelpText.RenderParsingErrorsTextAsLines(result, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);
            
            foreach (string s in errorMessages)
                Console.Error.WriteLine(s);
        }
    }
}