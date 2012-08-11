using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rohi.Apps.Core;
using Rohi.EBinder.DataAccess;
using TinyIoC;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Rohi.SPSync.Metro.Sync
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class AddSitePage : Rohi.SPSync.Metro.Common.BaseLayoutAwarePage
    {
        #region Properties

        private ISyncDriver _driver;
        private ISyncDriver Driver
        {
            get
            {
                if (_driver == null)
                {
                    var container = TinyIoCContainer.Current;
                    _driver = container.Resolve<ISyncDriver>();
                }
                return _driver;
            }
            set
            {
                if (value != null)
                    throw new ArgumentOutOfRangeException("value");

                if (_driver != null)
                {
                    _driver = null;
                }
            }
        }

        #endregion

        public AddSitePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private async void bnAddSite_Click(object sender, RoutedEventArgs e)
        {
            // do validation

            try
            {
                var browser = await DoLogin();
                DoSharePointSync(browser);
                ShowMessage("SharePoint Sync", "Synchronized successfully");
            }
            catch (Exception ex)
            {
                ShowError("Login", "Error occured login to SharePoint", ex);
            }
        }

        #region Login Handler
        private async Task<IDbBrowser> DoLogin()
        {
            Log.LogMessage("Loginx: {0}; {1}; {2}", tbSiteUrl.Text, tbLoginName.Text, tbPassword.Password);
            try
            {
                // Authenticate the site
                var browser = await Session.AuthenticateUser(
                                        Rohi.EBinder.Entities.ServerEdition.SP2010,
                                        tbSiteUrl.Text, tbLoginName.Text, tbPassword.Password,
                                        chkAutomaticLogin.IsChecked == true);
                return browser;
            }
            catch (Exception ex)
            {
                Log.LogException("Login Exception:", ex);
                ShowError("Login Failed", "Failed to login user", ex);
                return null;
            }
        }

        #endregion

        #region Download/Sync Handler

        private void DoSharePointSync(IDbBrowser browser)
        {
            try
            {
                Driver.Run(browser, chkAutomaticLogin.IsChecked == true);
            }
            catch (Exception ex)
            {
                ShowError("Sync Failed", "Sync failed due to: ", ex);
            }
        }

        #endregion
    }
}
