// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileLoggingService.cs" company="GSD Logic">
//   Copyright © 2017 GSD Logic. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExperimentalDVRService
{
    using System;
    using System.IO;

    public class FileLoggingService : ILoggingService
    {
        private readonly string fileName;

        public FileLoggingService(string fileName)
        {
            this.fileName = fileName;
        }

        public void Write(string message)
        {
            var directory = Path.GetDirectoryName(this.fileName);

            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
            var contents = string.Concat(timestamp, ' ', message, Environment.NewLine);
            File.AppendAllText(this.fileName, contents);
        }
    }
}