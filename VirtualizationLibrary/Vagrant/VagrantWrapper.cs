using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace OneClickDesktop.VirtualizationLibrary.Vagrant
{
    /// <summary>
    /// Klasa zajmuje się wykonywaniem polecen na specjalnie przygotowanym vagrantfile'u
    /// i udostepnia interfejs dla VirtualizationManagera.
    /// </summary>
    public class VagrantWrapper
    {
        private string vagrantfilePath;

        public VagrantWrapper(string filepath)
        {
            vagrantfilePath = filepath;
        }

        private (int, string) RunCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = command,  }; 
            Process proc = new Process() { StartInfo = startInfo, };
            proc.Start();
            proc.WaitForExit();
            return (proc.ExitCode, proc.StandardOutput.ReadToEnd());
        }
        
        public void VagrantUp(VagrantUpParameters parameters)
        {
            (int code, string output) = RunCommand($"VAGRANT_VAGRANTFILE={vagrantfilePath} vagrant {parameters.FormatForExecute()} up");
            
            //Kody bledow odnalezc w zrodle vagranta. Rozpoczac badanie tutaj: https://github.com/hashicorp/vagrant/blob/main/lib/vagrant/errors.rb
            switch (code)
            {
                case 0:
                    return;
                case 255:
                default:
                    //TODO: Dodac logi
                    Console.Error.Write(output);
                    throw new UnknownException();
                    break;
            }
        }
    }
}