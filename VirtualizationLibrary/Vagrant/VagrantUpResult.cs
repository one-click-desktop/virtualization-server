using System;

namespace OneClickDesktop.VirtualizationLibrary.Vagrant
{
    /// <summary>
    /// Błąd opisujący nieprawidlowe działania vagranta.
    /// Klasy pochodne opisują konkretne przypadki.
    /// </summary>
    public class VagrantUpException: Exception
    { }
    
    /// <summary>
    /// Nieznany błąd w czasie działania vagranta
    /// </summary>
    public class UnknownException: VagrantUpException { }
    
    /// <summary>
    /// Nieznany błąd w czasie działąnia vagranta
    /// </summary>
    public class BadArgumentsException: VagrantUpException { }
    
    /// <summary>
    /// Opisuje sytuację, gdy nie mozna połączyć sie z daemonem libvirta
    /// </summary>
    public class LibvirtConnectionException: VagrantUpException { }
    
    /// <summary>
    /// Opisuje systuacje, gdy brakuje w systemie skonfigurowanego firewalla
    /// </summary>
    public class NetworkFirewallException: VagrantUpException { }
}