// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowsService.cs" company="GSD Logic">
//   Copyright © 2017 GSD Logic. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExperimentalDVRService
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using System.ServiceProcess;

    /// <summary>
    /// Povides methods to install and uninstall a Windows Servce application.
    /// </summary>
    public static class WindowsService
    {
        /// <summary>
        /// Gets a value indicating whether the current process is a Windows Service.
        /// </summary>
        public static bool IsServiceProcess => Assembly.GetEntryAssembly()?.EntryPoint?.ReflectedType?.BaseType?.FullName == "System.ServiceProcess.ServiceBase";

        /// <summary>
        /// Performs the installation of a service.
        /// </summary>
        /// <param name="serviceName">The name used by the system to identify this service. This value must be identical to the <see cref="ServiceBase.ServiceName" /> of the service you want to install.</param>
        /// <param name="displayName">The friendly name that identifies the service to the user.</param>
        /// <param name="description">The description for the service.</param>
        /// <param name="servicePath">The path to the service executable.</param>
        /// <param name="arguments">The command line arguments for the service executable.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This is a bug in FxCop.")]
        public static void Install(string serviceName, string displayName, string description, string servicePath, string arguments)
        {
            Installer installer = null;
            ServiceProcessInstaller serviceProcessInstaller = null;
            ServiceInstaller serviceInstaller = null;
            TransactedInstaller transactedInstaller = null;

            try
            {
                serviceProcessInstaller = new ServiceProcessInstaller
                {
                    Account = ServiceAccount.LocalService
                };

                serviceInstaller = new ServiceInstaller
                {
                    ServiceName = serviceName,
                    DisplayName = displayName,
                    Description = description,
                    StartType = ServiceStartMode.Automatic
                };

                installer = new Installer();
                installer.Installers.Add(serviceProcessInstaller);
                installer.Installers.Add(serviceInstaller);

                var command = string.IsNullOrWhiteSpace(arguments) ? string.Format(CultureInfo.InvariantCulture, "/assemblypath=\"{0}\"", servicePath) : string.Format(CultureInfo.InvariantCulture, "/assemblypath=\"{0}\" {1}", servicePath, arguments);

                transactedInstaller = new TransactedInstaller();
                transactedInstaller.Installers.Add(installer);
                transactedInstaller.Context = new InstallContext(string.Empty, new[] { command });
                transactedInstaller.Install(new Hashtable());
            }
            finally
            {
                transactedInstaller?.Dispose();
                serviceInstaller?.Dispose();
                serviceProcessInstaller?.Dispose();
                installer?.Dispose();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the service is installed.
        /// </summary>
        /// <param name="serviceName">The name used by the system to identify the service. This value must be identical to the <see cref="ServiceBase.ServiceName" /> of the service.</param>
        /// <returns><see langword="true" /> if the service is instaleld; otherwise, <see langword="false" />.</returns>
        public static bool IsInstalled(string serviceName)
        {
            return ServiceController.GetServices().Any(s => s.ServiceName == serviceName);
        }

        /// <summary>
        /// Evaluates the command line arguments for service actions and terminates the application if processed.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="serviceFactory">The factory for to create an instance of the service to run.</param>
        public static void RunFromCommandLine(string[] args, Func<ServiceBase> serviceFactory)
        {
            if (serviceFactory == null)
            {
                throw new ArgumentNullException(nameof(serviceFactory));
            }

            if (IsServiceProcess)
            {
                var service = serviceFactory.Invoke();
                ServiceBase.Run(service);
                Environment.Exit(0);
            }

            if (args == null || args.Length != 1)
            {
                return;
            }

            switch (args[0].ToUpperInvariant())
            {
                case "/SERVICE":
                case "-SERVICE":
                {
                    var service = serviceFactory.Invoke();
                    ServiceBase.Run(service);
                    Environment.Exit(0);
                    break;
                }

                case "/INSTALL":
                case "-INSTALL":
                {
                    var assembly = Assembly.GetEntryAssembly();
                    var sourcePath = assembly.Location;
                    var fileName = Path.GetFileName(sourcePath);

                    if (fileName == null)
                    {
                        return;
                    }

                    var service = serviceFactory.Invoke();
                    var serviceName = service.ServiceName;
                    var displayName = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).OfType<AssemblyProductAttribute>().FirstOrDefault()?.Product ?? service.ServiceName;
                    var description = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description ?? string.Empty;
                    var company = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false).OfType<AssemblyCompanyAttribute>().FirstOrDefault()?.Company ?? string.Empty;
                    var servicePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), company, displayName, fileName);

                    InstallToProgramFiles(serviceName, displayName, description, sourcePath, servicePath, "-service");
                    Environment.Exit(0);
                    break;
                }

                case "/UNINSTALL":
                case "-UNINSTALL":
                {
                    var assembly = Assembly.GetEntryAssembly();
                    var sourcePath = assembly.Location;
                    var fileName = Path.GetFileName(sourcePath);

                    if (fileName == null)
                    {
                        return;
                    }

                    var service = serviceFactory.Invoke();
                    var serviceName = service.ServiceName;
                    var displayName = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).OfType<AssemblyProductAttribute>().FirstOrDefault()?.Product ?? service.ServiceName;
                    var description = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description ?? string.Empty;
                    var company = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false).OfType<AssemblyCompanyAttribute>().FirstOrDefault()?.Company ?? string.Empty;
                    var servicePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), company, displayName, fileName);

                    UninstallFromProgramFiles(serviceName, displayName, description, sourcePath, servicePath, "-service");
                    Environment.Exit(0);
                    break;
                }
            }
        }

        /// <summary>
        /// Removes the service from the computer.
        /// </summary>
        /// <param name="serviceName">The name used by the system to identify this service. This value must be identical to the <see cref="ServiceBase.ServiceName" /> of the service you want to install.</param>
        /// <param name="displayName">The friendly name that identifies the service to the user.</param>
        /// <param name="description">The description for the service.</param>
        /// <param name="servicePath">The path to the service executable.</param>
        /// <param name="arguments">The command line arguments for the service executable.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This is a bug in FxCop.")]
        public static void Uninstall(string serviceName, string displayName, string description, string servicePath, string arguments)
        {
            Installer installer = null;
            ServiceProcessInstaller serviceProcessInstaller = null;
            ServiceInstaller serviceInstaller = null;
            TransactedInstaller transactedInstaller = null;

            try
            {
                serviceProcessInstaller = new ServiceProcessInstaller
                {
                    Account = ServiceAccount.LocalService
                };

                serviceInstaller = new ServiceInstaller
                {
                    ServiceName = serviceName,
                    DisplayName = displayName,
                    Description = description,
                    StartType = ServiceStartMode.Automatic
                };

                installer = new Installer();
                installer.Installers.Add(serviceProcessInstaller);
                installer.Installers.Add(serviceInstaller);

                var command = string.IsNullOrWhiteSpace(arguments) ? string.Format(CultureInfo.InvariantCulture, "/assemblypath=\"{0}\"", servicePath) : string.Format(CultureInfo.InvariantCulture, "/assemblypath=\"{0}\" {1}", servicePath, arguments);

                transactedInstaller = new TransactedInstaller();
                transactedInstaller.Installers.Add(installer);
                transactedInstaller.Context = new InstallContext(string.Empty, new[] { command });
                transactedInstaller.Uninstall(null);
            }
            finally
            {
                transactedInstaller?.Dispose();
                serviceInstaller?.Dispose();
                serviceProcessInstaller?.Dispose();
                installer?.Dispose();
            }
        }

        /// <summary>
        /// Performs the installation of a service.
        /// </summary>
        /// <param name="serviceName">The name used by the system to identify this service. This value must be identical to the <see cref="ServiceBase.ServiceName" /> of the service you want to install.</param>
        /// <param name="displayName">The friendly name that identifies the service to the user.</param>
        /// <param name="description">The description for the service.</param>
        /// <param name="sourcePath">The source path to the service executable.</param>
        /// <param name="servicePath">The destination path to the service executable.</param>
        /// <param name="arguments">The command line arguments for the service executable.</param>
        private static void InstallToProgramFiles(string serviceName, string displayName, string description, string sourcePath, string servicePath, string arguments)
        {
            if (IsInstalled(serviceName))
            {
                return;
            }

            var sourceFileName = Path.GetFileName(sourcePath);

            if (sourceFileName == null)
            {
                return;
            }

            var tempPath = Path.Combine(Path.GetTempPath(), sourceFileName);

            if (sourcePath != tempPath)
            {
                File.Copy(sourcePath, tempPath, true);
                RunAsAdministrator(tempPath, "-INSTALL");
                return;
            }

            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                return;
            }

            var directory = Path.GetDirectoryName(servicePath);

            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(sourceFileName, servicePath, true);
            Install(serviceName, displayName, description, servicePath, arguments);

            using (ServiceController controller = new ServiceController(serviceName))
            {
                controller.Start();
            }
        }

        /// <summary>
        /// Starts a process with elevated privileges.
        /// </summary>
        /// <param name="fileName">The application to start.</param>
        /// <param name="arguments">The set of command-line arguments to use when starting the application.</param>
        private static void RunAsAdministrator(string fileName, string arguments)
        {
            using (Process process = new Process())
            {
                var startInfo = new ProcessStartInfo
                {
                    Verb = "runas",
                    FileName = fileName,
                    Arguments = arguments
                };

                process.StartInfo = startInfo;
                process.EnableRaisingEvents = true;

                try
                {
                    process.Start();
                    process.WaitForExit();
                }
                catch (Win32Exception)
                {
                }
            }
        }

        /// <summary>
        /// Removes the service from the computer.
        /// </summary>
        /// <param name="serviceName">The name used by the system to identify this service. This value must be identical to the <see cref="ServiceBase.ServiceName" /> of the service you want to install.</param>
        /// <param name="displayName">The friendly name that identifies the service to the user.</param>
        /// <param name="description">The description for the service.</param>
        /// <param name="sourcePath">The source path to the service executable.</param>
        /// <param name="servicePath">The destination path to the service executable.</param>
        /// <param name="arguments">The command line arguments for the service executable.</param>
        private static void UninstallFromProgramFiles(string serviceName, string displayName, string description, string sourcePath, string servicePath, string arguments)
        {
            if (!IsInstalled(serviceName))
            {
                return;
            }

            var sourceFileName = Path.GetFileName(sourcePath);

            if (sourceFileName == null)
            {
                return;
            }

            var tempPath = Path.Combine(Path.GetTempPath(), sourceFileName);

            if (sourcePath != tempPath)
            {
                File.Copy(sourcePath, tempPath, true);
                RunAsAdministrator(tempPath, "-UNINSTALL");
                return;
            }

            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                return;
            }

            Uninstall(serviceName, displayName, description, servicePath, arguments);

            if (File.Exists(servicePath))
            {
                File.Delete(servicePath);
            }

            var directory = Path.GetDirectoryName(servicePath);

            while (directory != null && Directory.Exists(directory) && Directory.GetFiles(directory).Length == 0)
            {
                Directory.Delete(directory);
                directory = Path.GetDirectoryName(directory);
            }
        }
    }
}