﻿#region Using

using System;
using System.Windows;
using System.Windows.Controls;

using NLib;

using DMT.Configurations;
using DMT.Services;

#endregion

namespace DMT.Controls.StatusBar
{
    /// <summary>
    /// Interaction logic for AppInfoStatus.xaml
    /// </summary>
    public partial class AppInfoStatus : UserControl
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppInfoStatus()
        {
            InitializeComponent();
        }

        #endregion

        #region Loaded/Unloaded

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateUI();
            /*
            TAConfigManager.Instance.ConfigChanged += ConfigChanged;
            TAUIConfigManager.Instance.ConfigChanged += UI_ConfigChanged;
            */
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            /*
            TAUIConfigManager.Instance.ConfigChanged -= UI_ConfigChanged;
            TAConfigManager.Instance.ConfigChanged -= ConfigChanged;
            */
        }

        #endregion

        #region Config Watcher Handlers

        private void ConfigChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void UI_ConfigChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        #endregion

        private void UpdateUI()
        {
            /*
            var statusCfg = TAUIConfigManager.Instance.AppInfo;
            if (null == statusCfg || !statusCfg.Visible)
            {
                // Hide Control.
                if (this.Visibility == Visibility.Visible) this.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Show Control.
                if (this.Visibility != Visibility.Visible) this.Visibility = Visibility.Visible;
            }
            */
            txtAppInfo.Text = ApplicationManager.Instance.Environments.Options.AppInfo.DisplayText;
        }
    }
}
