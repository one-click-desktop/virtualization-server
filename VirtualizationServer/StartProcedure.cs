using System;
using NLog;
using OneClickDesktop.VirtualizationServer.Services;

namespace OneClickDesktop.VirtualizationServer
{
    public class RunningServices: IDisposable
    {
        public VirtualizationManager virtualizationManager;
        public RequestReader requestReader;
        //Model sender?
        
        public void Dispose()
        {
            virtualizationManager?.Dispose();
        }
    }
    
    public static class StartProcedure
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        private static void PrepareVirtualizationManager(RunningServices result)
        {
            string libvirtUri = "qemu:///system";//[TODO][CONFIG] Wynieść do konfiguracji!
            string vagrantFile = "res/Vagrantfile";//[TODO][CONFIG] Wynieść do konfiguracji!
            result.virtualizationManager = new VirtualizationManager(libvirtUri, vagrantFile);
        }

        private static void PrepareRequestReader(RunningServices result)
        {
            result.requestReader = new RequestReader();
        }

        public static RunningServices InitializeVirtualizationServer()
        {
            logger.Info("Initializing Virtualization Server");

            RunningServices res = new RunningServices();
            PrepareVirtualizationManager(res);
            PrepareRequestReader(res);
            
            logger.Info("Virtualization successfuly initialized");
            return res;
        }
    }
}