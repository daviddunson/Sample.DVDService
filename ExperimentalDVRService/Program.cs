// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="GSD Logic">
//   Copyright © 2017 GSD Logic. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExperimentalDVRService
{
    using System;

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        private static void Main(string[] args)
        {
            WindowsService.RunFromCommandLine(args, () => new DvrService());

            var service = new DvrService();
            service.Run(args);

            bool isRunning = true;

            Console.WriteLine("DVR Service is running.");

            while (isRunning)
            {
                switch (Console.ReadLine()?.ToUpperInvariant())
                {
                    case "?":
                    case "HELP":
                        Console.WriteLine("STATUS    Show Windows Service status");
                        Console.WriteLine("INSTALL   Install as Windows Service");
                        Console.WriteLine("UNINSTALL Remove Windows Service");
                        Console.WriteLine("EXIT      Shutdown and exit");
                        break;

                    case "STATUS":
                        Console.WriteLine("Windows Service Name: {0}.", service.ServiceName);
                        Console.WriteLine("Windows Service is {0}.", WindowsService.IsInstalled(service.ServiceName) ? "installed" : "not installed");
                        break;

                    case "INSTALL":
                        WindowsService.RunFromCommandLine(new[] { "/INSTALL" }, () => service);
                        break;

                    case "UNINSTALL":
                        WindowsService.RunFromCommandLine(new[] { "/UNINSTALL" }, () => service);
                        break;

                    case "EXIT":
                        isRunning = false;
                        break;

                    default:
                        Console.WriteLine("?");
                        break;
                }
            }

            service.Stop();
        }
    }
}