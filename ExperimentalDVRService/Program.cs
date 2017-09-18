// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="GSD Logic">
//   Copyright © 2017 GSD Logic. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExperimentalDVRService
{
    using System;
    using System.Diagnostics;
    using System.IO;

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        private static void Main(string[] args)
        {
            var logpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"GSD Logic\Experimental DVR\service.log");
            WindowsService.Run(args, new DvrService(new FileLoggingService(logpath)));
            ServiceConsole.Run(args, new DvrService(new ConsoleLoggingService()));
        }
    }
}