using System.Collections.Generic;
using OneClickDesktop.BackendClasses.Model.States;

namespace OneClickDesktop.VirtualizationServer
{
    public static class Constants
    {
        public static class State
        {
            public static readonly HashSet<MachineState> MachineAvailableForShutdown = new HashSet<MachineState>()
            {
                MachineState.Free,
                MachineState.WaitingForShutdown
            };

            public static readonly HashSet<MachineState> MachineAvailableForSession = new HashSet<MachineState>()
            {
                MachineState.Free
            };
        }
    }
}