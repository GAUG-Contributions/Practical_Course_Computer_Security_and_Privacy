using SensorFeedback.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace SensorFeedback
{
    public partial class App : Application
    {      
        public App()
        {
            InitializeComponent();
        }

        protected override void OnStart()
        {
            // The app has to request the permission to obtain the location information
            RequestPermissionLocationAsync();

            // The app has to request the permission to access sensors
            RequestPermissionSensorAsync();

        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        /// <summary>
        /// The app has to request the permission to obtain the location information.
        /// </summary>
        private async void RequestPermissionLocationAsync()
        {
            var response = await PrivacyPermissionService.RequestAsync(PrivacyPrivilege.Location);
            if (response == PrivacyPermissionStatus.Denied)
            {
                Logger.Error("Location privilege denied!", "App.xaml.cs", "RequestPermissionLocationAsync");
                Application.Current.Quit();
            }
        }

        /// <summary>
        /// The app has to request the permission to access sensors.
        /// </summary>
        private async void RequestPermissionSensorAsync()
        {
            var response = await PrivacyPermissionService.RequestAsync(PrivacyPrivilege.HealthInfo);
            if (response == PrivacyPermissionStatus.Denied)
            {
                Logger.Error("Health privilege denied!", "App.xaml.cs", "RequestPermissionSensorAsync");
                Application.Current.Quit();
            }
        }

    }
}
