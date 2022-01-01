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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        private string vagrantfilePath;

        public VagrantWrapper(string filepath)
        {
            vagrantfilePath = filepath;
        }

        /// <summary>
        /// Wykonuje polecenie systemowe zgodne z parametrami na wejściu.
        /// Czeka do czasu zakończenia polecenia oraz wypisuje kod wykonania i stderr.
        /// </summary>
        /// <param name="startInfo">Parametry uruchomienia</param>
        /// <returns>Kod wyjścia oraz zawartośc stderr jezeli ustawione aby przechwytywać.</returns>
        private (int, string) RunCommand(ProcessStartInfo startInfo)
        {
            Process proc = new Process() { StartInfo = startInfo, };
            proc.Start();
            proc.WaitForExit();
            string stderr = proc.StandardError.ReadToEnd();
            
            return (proc.ExitCode, stderr);
        }
        
        /// <summary>
        /// Przygotowuje parametry uruchomieniowe pod polecenia vagrant na Vagrantfile
        /// </summary>
        /// <param name="command">Polecenie do wykonania w bashu</param>
        /// <param name="parameters">Parametry przekazywane do Vagrntfile</param>
        /// <returns>Parametry uruchomieniowe procesu</returns>
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
            startInfo.EnvironmentVariables["VAGRANT_DEFAULT_PROVIDER"] = "libvirt";
            parameters.DefineEnvironmentalVariables(startInfo.EnvironmentVariables);

            return startInfo;
        }
        
        /// <summary>
        /// Sprawdza błędy uruchomienia polecenia vagrantowego
        /// </summary>
        /// <param name="code">Kod wyjścia</param>
        /// <param name="stderr">Zawartośc stderr</param>
        /// <exception cref="VagrantException">Zgłąszany w wypadku błedu uruchomienia vagranta</exception>
        /// <exception cref="BadArgumentsException">Zgłaszany w przypadku błedu składniowego polecenia</exception>
        /// <exception cref="UnknownException">Zgłaszany w pozostałych przypadkach</exception>
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
                    logger.Error($"Vagrant reports unpredicted error with code {code}.");
                    throw new UnknownException(stderr);
                    break;
            }
        }
        
        /// <summary>
        /// Wykonuje polecenie vagrant up tworzac maszyne o podanych parametrach.
        /// W wypadku niepowodzenia staramy sie wyczyscic co sie da przy pomocy metody VagrantDestroy.
        /// </summary>
        /// <param name="parameters">Parametry uruchamianej maszyny</param>
        public void VagrantUp(VagrantParameters parameters)
        {
            (int code, string stderr) = RunCommand(PrepareForVagrantCommand("vagrant up", parameters));
            
            try
            {
                CheckErrors(code, stderr);
            }
            catch (VagrantException e)
            {
                BestEffortVagrantDestroy(parameters);
                throw;
            }
            
        }
        
        /// <summary>
        /// Niszcze aktualnie działającą maszyne o podanych parametrach
        /// </summary>
        /// <param name="parameters">Parametry maszyny do zniszczenia</param>
        public void VagrantDestroy(VagrantParameters parameters)
        {
            (int code, string stderr) = RunCommand(PrepareForVagrantCommand("vagrant destroy -f", parameters));
            CheckErrors(code, stderr);
        }

        public void BestEffortVagrantDestroy(VagrantParameters parameters)
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
        }
    }
}