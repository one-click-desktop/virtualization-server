using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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

        private (int, string) RunCommand(ProcessStartInfo startInfo)
        {
            Process proc = new Process() { StartInfo = startInfo, };
            proc.Start();
            proc.WaitForExit();
            string stderr = proc.StandardError.ReadToEnd();
            
            return (proc.ExitCode, stderr);
        }

        private ProcessStartInfo PrepareForVagrantCommand(string command, VagrantParameters parameters)
        {
            ProcessStartInfo startInfo =
                new ProcessStartInfo()
                {
                    FileName = "/bin/bash",
                    ArgumentList = { "-c", command },
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
            startInfo.EnvironmentVariables["VAGRANT_VAGRANTFILE"] = vagrantfilePath;
            parameters.DefineEnvironmentalVariables(startInfo.EnvironmentVariables);

            return startInfo;
        }
 
        private void CheckErrors(int code, string stderr)
        {
            //Kody bledow odnalezc w zrodle vagranta. Rozpoczac badanie tutaj: https://github.com/hashicorp/vagrant/blob/main/lib/vagrant/errors.rb
            switch (code)
            {
                case 0:
                    return;
                case 1:
                    throw new VagrantException(stderr);
                case 127:
                    throw new BadArgumentsException(stderr);
                case 255:
                default:
                    //TODO: Dodac logi
                    Console.Error.Write(stderr);
                    throw new UnknownException(stderr);
                    break;
            }
        }
        
        public void VagrantUp(VagrantParameters parameters)
        {
            (int code, string stderr) = RunCommand(PrepareForVagrantCommand("vagrant up", parameters));
            
            try
            {
                CheckErrors(code, stderr);
            }
            catch (VagrantException e)
            {
                //Sprobuj wyczyscic co sie da po blednym starcie maszyny
                try
                {
                    VagrantDestroy(parameters);
                }
                catch (VagrantException)
                {
                    //Ignorujemy wyjatki z destroya - best effort
                }

                throw;
            }
            
        }

        public void VagrantDestroy(VagrantParameters parameters)
        {
            (int code, string stderr) = RunCommand(PrepareForVagrantCommand("vagrant destroy -f", parameters));
            CheckErrors(code, stderr);
        }
    }
}