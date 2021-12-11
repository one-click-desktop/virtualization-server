using System;
using System.Threading.Tasks;
using OneClickDesktop.VirtualizationServer.Services;

namespace OneClickDesktop.VirtualizationServer
{
    class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main()
        {
            try
            {
                Logger.Info("Hello world");

                RequestReader reader = new RequestReader();
                reader.HelloWorld();

                throw new Exception("Test exception");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Goodbye cruel world");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}