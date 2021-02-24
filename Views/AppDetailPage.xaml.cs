using System;
using Tizen.Applications;
using Tizen.Wearable.CircularUI.Forms;

using Xamarin.Forms;

namespace WatchOut.Views
{
    public partial class AppDetailPage : ContentPage
    {
        public AppDetailPage(string appId)
        {
            InitializeComponent();
            ApplicationInfo appInfo = new ApplicationInfo(appId);
            ApplicationRunningContext appContext = null;
            try
            {
                appContext = new ApplicationRunningContext(appId);
            }
            catch (Exception e)
            {
                Tizen.Log.Error("Tizen.Applications", e.Message, "", "", 0);
            }
            appLabel.Text = appInfo.Label;
            //appCategory.Text = appInfo.Categories.ToString();
            appType.Text = appInfo.ApplicationType;
            appPID.Text = "PID: " + (appContext == null ? "Not running" : appContext.ProcessId.ToString());
        }
    }
}
