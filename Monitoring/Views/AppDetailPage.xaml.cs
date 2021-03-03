using System;
using Tizen.Applications;
using Tizen.Sensor;
using Tizen.Security;
using Tizen.Wearable.CircularUI.Forms;
using WatchOut.Services;
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
                Logger.Error(e.Message);
            }

            bool isSensing = false;
            try
            {
                const string hrPrivilege = "http://tizen.org/privilege/healthinfo";
                CheckResult result = PrivacyPrivilegeManager.CheckPermission(hrPrivilege);
                switch (result)
                {
                    case CheckResult.Allow:
                        var sensor = new HeartRateMonitor();
                        sensor.Start();
                        isSensing = sensor.IsSensing;
                        break;
                    case CheckResult.Deny:
                        break;
                    case CheckResult.Ask:
                        PrivacyPrivilegeManager.RequestPermission(hrPrivilege);
                        break;
                }
                
            }
            catch (Exception)
            {
                /// Accelerometer is not supported in the current device.
                /// You can also check whether the accelerometer is supported with the following property:
                /// var supported = Accelerometer.IsSupported;
            }

            appLabel.Text = appInfo.Label;
            appPID.Text = "PID: " + (appContext == null ? "Not running" : appContext.ProcessId.ToString());
            appSensors.Text = isSensing ? "Reads HR Sensors" : "Not reading HR Sensors";
        }
    }
}
