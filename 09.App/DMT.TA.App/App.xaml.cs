﻿#define ENABLE_COUPON_SYNC_SERVICE

#region Using

using System;
using System.Windows;

using NLib;
using NLib.Logs;

using DMT.Configurations;
using DMT.Services;

#endregion

namespace DMT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Services.TAWebServer appServ = null;

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
                    ProductName = AppConsts.Application.TA.ApplicationName,
                    /* For Application Version */
                    Version = AppConsts.Application.TA.Version,
                    Minor = AppConsts.Application.TA.Minor,
                    Build = AppConsts.Application.TA.Build,
                    LastUpdate = AppConsts.Application.TA.LastUpdate
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

            // Load Config service.
            TAConfigManager.Instance.LoadConfig();
            TAConfigManager.Instance.ConfigChanged += Service_ConfigChanged;
            // Setup config reference to all rest client class.
            // TODO: Need Check TOD App Implements
            /*
            Services.Operations.TOD.Config = TAConfigManager.Instance;
            Services.Operations.TOD.DMT = TAConfigManager.Instance; // required for NetworkId
            */
            Services.Operations.TAxTOD.Config = TAConfigManager.Instance;
            Services.Operations.TAxTOD.DMT = TAConfigManager.Instance; // required for NetworkId

            Services.Operations.SCW.Config = TAConfigManager.Instance;
            Services.Operations.SCW.DMT = TAConfigManager.Instance; // required for NetworkId
            TAConfigManager.Instance.Start(); // Start File Watcher.

            // Start App Notify Server.
            appServ = new Services.TAWebServer();
            appServ.Start();

            // Init NotifyService event.
            TANotifyService.Instance.TSBChanged += TSBChanged;
            TANotifyService.Instance.TSBShiftChanged += TSBShiftChanged;

            // Start coupon sync service.
#if ENABLE_COUPON_SYNC_SERVICE
            CouponSyncService.Instance.Start();
#endif

            // Notify all TOD Apps that TSB Shift is changed.
            TODClientManager.Instance.TODTSBShiftChanged();

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
            // Shutdown coupon sync service.
#if ENABLE_COUPON_SYNC_SERVICE
            CouponSyncService.Instance.Shutdown();
#endif

            // Release NotifyService event.
            TANotifyService.Instance.TSBChanged -= TSBChanged;
            TANotifyService.Instance.TSBShiftChanged -= TSBShiftChanged;

            // Shutdown File Watcher.
            TAConfigManager.Instance.Shutdown();

            if (null != appServ)
            {
                appServ.Shutdown();
            }
            appServ = null;

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
            // TODO: Need Check TOD App Implements
            /*
            Services.Operations.TOD.Config = TAConfigManager.Instance;
            Services.Operations.TOD.DMT = TAConfigManager.Instance; // required for NetworkId
            */
            Services.Operations.TAxTOD.Config = TAConfigManager.Instance;
            Services.Operations.TAxTOD.DMT = TAConfigManager.Instance; // required for NetworkId

            Services.Operations.SCW.Config = TAConfigManager.Instance;
            Services.Operations.SCW.DMT = TAConfigManager.Instance; // required for NetworkId
        }

        private void TSBChanged(object sender, EventArgs e)
        {
            RuntimeManager.Instance.RaiseTSBChanged();
        }

        private void TSBShiftChanged(object sender, EventArgs e)
        {
            // Notify all TOD Apps that TSB Shift is changed.
            TODClientManager.Instance.TODTSBShiftChanged();

            RuntimeManager.Instance.RaiseTSBShiftChanged();
        }
    }
}
