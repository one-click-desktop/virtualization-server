using System;

namespace OneClickDesktop.VirtualizationLibrary.Vagrant
{
    /// <summary>
    /// Błąd opisujący nieprawidlowe działania vagranta.
    /// Klasy pochodne opisują konkretne przypadki.
    /// </summary>
    public class VagrantUpException : Exception
    {
        public VagrantUpException()
        {
        }

        public VagrantUpException(string? message) : base(message)
        {
        }

        public VagrantUpException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
    
    /// <summary>
    /// Nieznany błąd w czasie działania vagranta
    /// </summary>
    public class UnknownException: VagrantUpException
    {
        public UnknownException()
        {
        }

        public UnknownException(string? message) : base(message)
        {
        }

        public UnknownException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
    
    /// <summary>
    /// Nieznany błąd w czasie działąnia vagranta
    /// </summary>
    public class BadArgumentsException: VagrantUpException
    {
        public BadArgumentsException()
        {
        }

        public BadArgumentsException(string? message) : base(message)
        {
        }

        public BadArgumentsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
    
    /// <summary>
    /// Opisuje sytuację, gdy nie mozna połączyć sie z daemonem libvirta
    /// </summary>
    public class LibvirtConnectionException: VagrantUpException { }
    
    /// <summary>
    /// Opisuje systuacje, gdy brakuje w systemie skonfigurowanego firewalla
    /// </summary>
    public class NetworkFirewallException: VagrantUpException { }
}