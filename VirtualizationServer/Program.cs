using System;
using System.Threading.Tasks;
using OneClickDesktop.VirtualizationServer.Services;

namespace OneClickDesktop.VirtualizationServer
{
    class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static RunningServices services;

        public static void Main()
        {
            try
            {
                services = StartProcedure.InitializeVirtualizationServer();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unhandled Expetion - server is going down");
            }
            finally
            {
                services?.Dispose();
                NLog.LogManager.Shutdown();
            }
        }
    }
}