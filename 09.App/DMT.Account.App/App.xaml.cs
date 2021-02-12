﻿#region Using

using System;
using System.Windows;

using NLib;
using NLib.Logs;

using DMT.Configurations;

#endregion

namespace DMT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// OnStartup.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Console.WriteLine("OnStartUp");
            if (null != AppDomain.CurrentDomain)
            {
                if (null != System.Threading.Thread.CurrentContext)
                {
                    Console.WriteLine("Thread CurrentContext is not null.");
                }
            }

            #region Create Application Environment Options

            EnvironmentOptions option = new EnvironmentOptions()
            {
                /* Setup Application Information */
                AppInfo = new NAppInformation()
                {
                    /*  This property is required */
                    CompanyName = "DMT",
                    /*  This property is required */
                    ProductName = AppConsts.Application.Account.ApplicationName,
                    /* For Application Version */
                    Version = AppConsts.Application.Account.Version,
                    Minor = AppConsts.Application.Account.Minor,
                    Build = AppConsts.Application.Account.Build,
                    LastUpdate = AppConsts.Application.Account.LastUpdate
                },
                /* Setup Storage */
                Storage = new NAppStorage()
                {
                    StorageType = NAppFolder.ProgramData
                },
                /* Setup Behaviors */
                Behaviors = new NAppBehaviors()
                {
                    /* Set to true for allow only one instance of application can execute an runtime */
                    IsSingleAppInstance = true,
                    /* Set to true for enable Debuggers this value should always be true */
                    EnableDebuggers = true
                }
            };

            #endregion

            #region Setup Option to Controller and check instance

            WpfAppContoller.Instance.Setup(option);

            if (option.Behaviors.IsSingleAppInstance &&
                WpfAppContoller.Instance.HasMoreInstance)
            {
                return;
            }

            #endregion

            #region Init Preload classes

            ApplicationManager.Instance.Preload(() =>
            {

            });

            #endregion

            // Start log manager
            LogManager.Instance.Start();

            // Start local database (account).
            Services.AccountDbServer.Instance.Start();

            // Load Config service.
            AccountConfigManager.Instance.LoadConfig();
            AccountConfigManager.Instance.ConfigChanged += Service_ConfigChanged;
            Services.Operations.TAxTOD.Config = AccountConfigManager.Instance;
            Services.Operations.TAxTOD.DMT = AccountConfigManager.Instance; // required for NetworkId
            Services.Operations.SCW.Config = AccountConfigManager.Instance;
            Services.Operations.SCW.DMT = AccountConfigManager.Instance; // required for NetworkId
            AccountConfigManager.Instance.Start(); // Start File Watcher.

            // Start SCWMQ
            Services.SCWMQService.Instance.Start();

            // Start RabbitMQ
            Services.RabbitMQService.Instance.RabbitMQ = AccountConfigManager.Instance.RabbitMQ;
            Services.RabbitMQService.Instance.Start();

            Window window = null;
            window = new MainWindow();

            if (null != window)
            {
                WpfAppContoller.Instance.Run(window);
            }
        }
        /// <summary>
        /// OnExit
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            AccountConfigManager.Instance.Shutdown();

            // Shutdown RabbitMQ.
            Services.RabbitMQService.Instance.Shutdown();

            // Shutdown SCWMQ
            Services.SCWMQService.Instance.Shutdown();

            // Shutdown local database (account).
            Services.AccountDbServer.Instance.Shutdown();

            // Shutdown log manager
            LogManager.Instance.Shutdown();

            // Wpf shutdown process required exit code.

            /* If auto close the single instance must be true */
            bool autoCloseProcess = true;
            WpfAppContoller.Instance.Shutdown(autoCloseProcess, e.ApplicationExitCode);

            base.OnExit(e);
        }

        private void Service_ConfigChanged(object sender, EventArgs e)
        {
            // When Service Config file changed.

            // Update all related service operations.
            Services.Operations.TAxTOD.Config = AccountConfigManager.Instance;
            Services.Operations.TAxTOD.DMT = AccountConfigManager.Instance; // required for NetworkId
            Services.Operations.SCW.Config = AccountConfigManager.Instance;
            Services.Operations.SCW.DMT = AccountConfigManager.Instance; // required for NetworkId

            // RabbitMQ
            Services.RabbitMQService.Instance.Shutdown(); // Shutdown
            // Reload config.
            Services.RabbitMQService.Instance.RabbitMQ = AccountConfigManager.Instance.RabbitMQ;
            Services.RabbitMQService.Instance.Start(); // Start
        }
    }
}
