﻿#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Markup;

using DMT.Configurations;
using DMT.Models;
using DMT.Services;
using DMT.Controls;

using NLib.Services;
using NLib.Reflection;
using System.Threading;
using System.Windows.Threading;

#endregion

namespace DMT.TOD.Pages.TollAdmin
{
    /// <summary>
    /// Interaction logic for EMVQRCodeListPage.xaml
    /// </summary>
    public partial class EMVQRCodeListPage : UserControl
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public EMVQRCodeListPage()
        {
            InitializeComponent();
        }

        #endregion

        #region Internal Variables

        //private CultureInfo culture = new CultureInfo("th-TH") { DateTimeFormat = { Calendar = new ThaiBuddhistCalendar() } };
        private CultureInfo culture = new CultureInfo("th-TH");
        private XmlLanguage language = XmlLanguage.GetLanguage("th-TH");

        private string _laneFilter = string.Empty;

        private CurrentTSBManager manager = null;

        #endregion

        #region Loaded/Unloaded

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Setup DateTime Picker.
            dtEntryDate.CultureInfo = culture;
            dtEntryDate.Language = language;
            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region RadioButton Handlers

        private void rbEMV_Click(object sender, RoutedEventArgs e)
        {
            RefreshEMV_QRCODE();
        }

        private void rbQRCode_Click(object sender, RoutedEventArgs e)
        {
            RefreshEMV_QRCODE();
        }

        #endregion

        #region DateTime Picker Handlers

        private void dtEntryDate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RefreshEMV_QRCODE();
        }

        #endregion

        #region TextBox Handlers

        private void txtLaneNo_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var currFilter = txtLaneNo.Text.Trim();
            if (e.Key == System.Windows.Input.Key.Enter ||
                e.Key == System.Windows.Input.Key.Return)
            {
                if (_laneFilter != currFilter)
                {
                    _laneFilter = currFilter;
                    RefreshEMV_QRCODE();
                    e.Handled = true;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                txtLaneNo.Text = string.Empty;
                e.Handled = true;
            }
        }

        private void txtLaneNo_GotFocus(object sender, RoutedEventArgs e)
        {
            _laneFilter = txtLaneNo.Text.Trim();
        }

        private void txtLaneNo_LostFocus(object sender, RoutedEventArgs e)
        {
            var currFilter = txtLaneNo.Text.Trim();
            if (_laneFilter != currFilter)
            {
                _laneFilter = currFilter;
                RefreshEMV_QRCODE();
            }
        }

        private void txtSearchUserId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter ||
                e.Key == System.Windows.Input.Key.Return)
            {
                SearchUser();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                ResetSelectUser();
                RefreshEMV_QRCODE();
                e.Handled = true;
            }
        }

        #endregion

        #region Button Handlers

        private void cmdBack_Click(object sender, RoutedEventArgs e)
        {
            GotoMainMenu();
        }

        private void cmdPaymentClear_Click(object sender, RoutedEventArgs e)
        {
            dtEntryDate.Value = DateTime.Now.Date;
            txtLaneNo.Text = string.Empty;
            RefreshEMV_QRCODE();
        }

        private void cmdPaymentSearch_Click(object sender, RoutedEventArgs e)
        {
            RefreshEMV_QRCODE();
        }

        private void cmdUserSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchUser();
        }

        #endregion

        #region Private Methods

        private void GotoMainMenu()
        {
            // Main Menu Page
            var page = TODApp.Pages.MainMenu;
            PageContentManager.Instance.Current = page;
        }

        private void Reset()
        {
            manager.User = null;
            if (null != manager && null != manager.Payments)
            {
                manager.Payments.EnableLaneFilter = true;
            }

            dtEntryDate.DefaultValue = DateTime.Now;
            dtEntryDate.Value = DateTime.Now.Date;
            // Set Bindings User Selection.
            txtUserId.DataContext = manager;
            txtUserName.DataContext = manager;
        }

        private void ResetSelectUser()
        {
            manager.User = null;
            txtSearchUserId.Text = string.Empty;
        }

        private void SearchUser()
        {
            string userId = txtSearchUserId.Text.Trim();
            var result = TODAPI.SearchUser(userId, TODApp.Permissions.TC);
            if (!result.IsCanceled && null != manager)
            {
                manager.User = result.User;
                if (null != manager.User)
                {
                    txtSearchUserId.Text = string.Empty;
                }
                RefreshEMV_QRCODE();
            }
        }

        private void RefreshEMV_QRCODE()
        {
            grid.ItemsSource = null;

            if (!dtEntryDate.Value.HasValue)
            {
                dtEntryDate.Focus();
                return;
            }

            DateTime dt1 = dtEntryDate.Value.Value.Date;
            DateTime dt2 = dt1.AddDays(1);

            if (null == manager || null == manager.Payments) return;

            manager.Payments.ViewMode = ViewModes.TSB; // View all.
            manager.Payments.PaymentType = (rbEMV.IsChecked.Value) ? PaymentTypes.EMV : PaymentTypes.QRCode;
            manager.Payments.Begin = dt1;
            manager.Payments.End = dt2;
            manager.Payments.Filter = _laneFilter;
            manager.Payments.Refresh();

            if (rbEMV.IsChecked.Value)
            {
                grid.ItemsSource = manager.Payments.EMVItems;
            }
            else
            {
                grid.ItemsSource = manager.Payments.QRCodeItems;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Setup.
        /// </summary>
        /// <param name="user">The User instance.</param>
        public void Setup(User user)
        {
            if (null == manager)
            {
                manager = new CurrentTSBManager();
            }

            Reset();
            ResetSelectUser();
            txtLaneNo.Text = string.Empty;

            // Focus on search textbox.
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                txtSearchUserId.SelectAll();
                txtSearchUserId.Focus();
            }));
        }

        #endregion
    }
}
