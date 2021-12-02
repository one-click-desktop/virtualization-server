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
            string output = proc.StandardError.ReadToEnd() + proc.StandardOutput.ReadToEnd();
            
            return (proc.ExitCode, output);
        }
        
        public void VagrantUp(VagrantParameters parameters)
        {
            ProcessStartInfo startInfo =
                new ProcessStartInfo()
                {
                    FileName = "/bin/bash",
                    ArgumentList = { "-c", "vagrant up" },
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
            startInfo.EnvironmentVariables["VAGRANT_VAGRANTFILE"] = vagrantfilePath;
            startInfo.EnvironmentVariables["OCD_BOX_NAME"] = parameters.VagrantBox;
            startInfo.EnvironmentVariables["OCD_VM_NAME"] = parameters.BoxName;
            startInfo.EnvironmentVariables["OCD_CPUS"] = parameters.CpuCores.ToString();
            startInfo.EnvironmentVariables["OCD_MEMORY"] = parameters.Memory.ToString();
            startInfo.EnvironmentVariables["OCD_HOSTNAME"] = parameters.Hostname;
            
            (int code, string output) = RunCommand(startInfo);
            
            //Kody bledow odnalezc w zrodle vagranta. Rozpoczac badanie tutaj: https://github.com/hashicorp/vagrant/blob/main/lib/vagrant/errors.rb
            switch (code)
            {
                case 0:
                    return;
                case 127:
                    throw new BadArgumentsException(output);
                case 255:
                default:
                    //TODO: Dodac logi
                    Console.Error.Write(output);
                    throw new UnknownException(output);
                    break;
            }
        }

        public void VagrantDestroy(VagrantParameters parameters)
        {
            ProcessStartInfo startInfo =
                new ProcessStartInfo()
                {
                    FileName = "/bin/bash",
                    ArgumentList = { "-c", "vagrant destroy -f" },
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
            startInfo.EnvironmentVariables["VAGRANT_VAGRANTFILE"] = vagrantfilePath;
            startInfo.EnvironmentVariables["OCD_BOX_NAME"] = parameters.VagrantBox;
            startInfo.EnvironmentVariables["OCD_VM_NAME"] = parameters.BoxName;
            startInfo.EnvironmentVariables["OCD_CPUS"] = parameters.CpuCores.ToString();
            startInfo.EnvironmentVariables["OCD_MEMORY"] = parameters.Memory.ToString();
            startInfo.EnvironmentVariables["OCD_HOSTNAME"] = parameters.Hostname;
            
            (int code, string output) = RunCommand(startInfo);

            if (code > 0)
                throw new UnknownException();
        }
    }
}