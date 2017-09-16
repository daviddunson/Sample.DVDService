// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DvrService.cs" company="GSD Logic">
//   Copyright © 2017 GSD Logic. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExperimentalDVRService
{
    using System.ComponentModel;
    using System.ServiceProcess;

    [DesignerCategory(@"Code")]
    public partial class DvrService : ServiceBase
    {
        public DvrService()
        {
            this.InitializeComponent();
            this.ServiceName = "ExperimentalDVRService";
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}