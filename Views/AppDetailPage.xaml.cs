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
            appLabel.Text = appInfo.Label;
            appType.Text = appInfo.ApplicationType;
        }
    }
}
