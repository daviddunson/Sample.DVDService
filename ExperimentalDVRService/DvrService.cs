// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DvrService.cs" company="GSD Logic">
//   Copyright © 2017 GSD Logic. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExperimentalDVRService
{
    using System;
    using System.ComponentModel;
    using System.ServiceProcess;

    [DesignerCategory(@"Code")]
    public class DvrService : ServiceBase
    {
        private readonly ILoggingService log;

        public DvrService(ILoggingService log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            this.log = log;
            this.ServiceName = "ExperimentalDVRService";
        }

        protected override void OnStart(string[] args)
        {
            this.log.Write("DVR Service Starting.");
        }

        protected override void OnStop()
        {
            this.log.Write("DVR Service Stopping.");
        }
    }
}