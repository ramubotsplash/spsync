using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Rohi.EBinder.DataAccess;
using Rohi.EBinder.Entities;
using TinyIoC;
using Windows.UI.Popups;

namespace Rohi.SPSync.Metro.Common
{
    [Windows.Foundation.Metadata.WebHostHidden]
    public class BaseLayoutAwarePage : LayoutAwarePage
    {
        public IAppSession Session
        {
            get
            {
                var container = TinyIoCContainer.Current;
                return container.Resolve<IAppSession>();
            }
        }

        private IBinderSettings BinderSettings
        {
            get
            {
                var container = TinyIoCContainer.Current;
                return container.Resolve<IBinderSettings>();
            }
        }

        public void ShowError(string title, string message, Exception ex)
        {
            if (ex != null)
            {
                bool hasMessage = false;
                if (ex is FaultException)
                {
                    FaultException faultException = (FaultException)ex;
                    MessageFault msgFault = faultException.CreateMessageFault();
                    if (msgFault.HasDetail)
                    {
                        hasMessage = true;
                        var detail = msgFault.GetDetail<XElement>();
                        message += string.Format(". Error: {0}.", detail.Value);
                    }
                }

                if (!hasMessage)
                {
                    message += string.Format(". Error: {0}.", ex.Message);

                    var innerEx = ex.InnerException;
                    while(innerEx != null)
                    {
                        message += string.Format(" {0}.", innerEx.Message);
                        innerEx = innerEx.InnerException;
                    }
                }
            }

            ShowMessage(title, message);
        }

        public async void ShowMessage(string title, string message)
        {
            // This method will be called on the app's UI thread, which allows for actions like manipulating
            // the UI or showing error dialogs

            // Display a dialog indicating to the user that a corrective action needs to occur
            var messageDialog = new Windows.UI.Popups.MessageDialog(message, title);

            // Add commands and set their command ids
            messageDialog.Commands.Add(new UICommand("OK", null, 0));

            // Set the command that will be invoked by default
            // messageDialog.DefaultCommandIndex = 1;


            await messageDialog.ShowAsync();
        }
    }
}
