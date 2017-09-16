// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceBaseExtensions.cs" company="GSD Logic">
//   Copyright © 2017 GSD Logic. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExperimentalDVRService
{
    using System;
    using System.Reflection;
    using System.ServiceProcess;

    public static class ServiceBaseExtensions
    {
        public static void Run(this ServiceBase service, string[] args)
        {
            var type = service.GetType();
            var start = type.GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string[]) }, null);
            start.Invoke(service, new object[] { args });
        }
    }
}