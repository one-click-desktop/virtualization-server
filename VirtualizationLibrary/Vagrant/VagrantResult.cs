using System;

namespace OneClickDesktop.VirtualizationLibrary.Vagrant
{
    /// <summary>
    /// Błąd opisujący nieprawidlowe działania vagranta.
    /// Klasy pochodne opisują konkretne przypadki.
    /// </summary>
    public class VagrantException : Exception
    {
        public VagrantException()
        {
        }

        public VagrantException(string? message) : base(message)
        {
        }

        public VagrantException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
    
    /// <summary>
    /// Nieznany błąd w czasie działania vagranta
    /// </summary>
    public class UnknownException: VagrantException
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
    public class BadArgumentsException: VagrantException
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
    public class LibvirtConnectionException: VagrantException { }
    
    /// <summary>
    /// Opisuje systuacje, gdy brakuje w systemie skonfigurowanego firewalla
    /// </summary>
    public class NetworkFirewallException: VagrantException { }
}