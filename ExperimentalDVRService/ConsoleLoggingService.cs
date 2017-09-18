// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleLoggingService.cs" company="GSD Logic">
//   Copyright © 2017 GSD Logic. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExperimentalDVRService
{
    using System;

    public class ConsoleLoggingService : ILoggingService
    {
        public void Write(string message)
        {
            Console.WriteLine(message);
        }
    }
}