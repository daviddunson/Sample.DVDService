// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceConsole.cs" company="GSD Logic">
//   Copyright © 2017 GSD Logic. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExperimentalDVRService
{
    using System;
    using System.ServiceProcess;

    public class ServiceConsole
    {
        private readonly string[] args;
        private readonly ServiceBase service;
        private bool isRunning;
        private bool isStarted;

        /// <summary>
        /// Evaluates the command line arguments for service actions and terminates the application if processed.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="service">The instance of the service to run.</param>
        public ServiceConsole(string[] args, ServiceBase service)
        {
            this.args = args;
            this.service = service;
        }

        /// <summary>
        /// Evaluates the command line arguments for service actions and terminates the application if processed.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="service">The instance of the service to run.</param>
        public static void Run(string[] args, ServiceBase service)
        {
            var console = new ServiceConsole(args, service);

            if (WindowsService.GetController(service.ServiceName) == null)
            {
                console.StartConsole();
            }

            console.Run();
        }

        private void InstallService()
        {
            this.StopConsole();
            WindowsService.Install(this.service);
        }

        private void Run()
        {
            this.isRunning = true;

            while (this.isRunning)
            {
                switch (Console.ReadLine()?.ToUpperInvariant())
                {
                    case "HELP":
                        this.ShowHelp();
                        break;

                    case "?":
                    case "STATUS":
                        this.ShowStatus();
                        break;

                    case "INSTALL":
                        this.InstallService();
                        break;

                    case "UNINSTALL":
                        this.UninstallService();
                        break;

                    case "UPDATE":
                        this.UpdateService();
                        break;

                    case "START":
                        this.Start();
                        break;

                    case "STOP":
                        this.Stop();
                        break;

                    case "STARTSERVICE":
                        this.StartService();
                        break;

                    case "STOPSERVICE":
                        this.StopService();
                        break;

                    case "STARTCONSOLE":
                        this.StartConsole();
                        break;

                    case "STOPCONSOLE":
                        this.StopConsole();
                        break;

                    case "EXIT":
                        this.isRunning = false;
                        break;

                    default:
                        Console.WriteLine("Unknown command.");
                        break;
                }
            }

            if (this.isStarted)
            {
                this.service.Stop();
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("status|?         Show service status");
            Console.WriteLine("install          Installs Windows Service");
            Console.WriteLine("uninstall        Uninstalls Windows Service");
            Console.WriteLine("update           Updates Windows Windows Service");
            Console.WriteLine("start            Starts Active Service");
            Console.WriteLine("stops            Stops Active Service");
            Console.WriteLine("startservice     Starts Windows Service");
            Console.WriteLine("stopservice      Stops Windows Service");
            Console.WriteLine("startconsole     Starts Console Service");
            Console.WriteLine("stopconsole      Stops Console Service");
            Console.WriteLine("exit             Shutdown and exit");
        }

        private void ShowStatus()
        {
            Console.WriteLine("Service Name: {0}", this.service.ServiceName);
            Console.WriteLine("Windows Service: {0}", WindowsService.GetController(this.service.ServiceName)?.Status.ToString() ?? "Not Installed");
            Console.WriteLine("Console Service: {0}", this.isStarted ? "Running" : "Stopped");
        }

        private void Start()
        {
            if (WindowsService.GetController(this.service.ServiceName) == null)
            {
                this.StartConsole();
            }
            else
            {
                this.StartService();
            }
        }

        private void StartConsole()
        {
            if (!this.isStarted)
            {
                this.StopService();
                this.service.Run(this.args);
                this.isStarted = true;
            }
        }

        private void StartService()
        {
            var controller = WindowsService.GetController(this.service.ServiceName);

            if (controller != null && controller.Status != ServiceControllerStatus.Running)
            {
                this.StopConsole();
                controller.Start();
            }
        }

        private void Stop()
        {
            this.StopService();
            this.StopConsole();
        }

        private void StopConsole()
        {
            if (this.isStarted)
            {
                this.service.Stop();
                this.isStarted = false;
            }
        }

        private void StopService()
        {
            var controller = WindowsService.GetController(this.service.ServiceName);

            if (controller != null && controller.Status != ServiceControllerStatus.Stopped)
            {
                controller.Stop();
            }
        }

        private void UninstallService()
        {
            WindowsService.Uninstall(this.service);
        }

        private void UpdateService()
        {
            this.UninstallService();
            this.InstallService();
        }
    }
}