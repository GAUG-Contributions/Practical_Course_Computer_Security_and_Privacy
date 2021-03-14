using SensorFeedback.Services;
using System;
using Xamarin.Forms;

namespace SensorFeedback.Views
{
    public partial class MonitoringSettingsPage : ContentPage
    {
        private RandomSensingService _randomSensingService;

        /// <summary>
        /// Constructor
        /// </summary>
        public MonitoringSettingsPage()
        {
            InitializeComponent();
            _randomSensingService = RandomSensingService.GetInstance;
        }

        public async void OnButtonSuspendClickedAsync(object sender, EventArgs e)
        {
            // Stop the randomization of services gathering sensor data
             _randomSensingService.AllowSensing(false);
            TimeSpan timer = new TimeSpan((int)StepperH.Value, (int)StepperM.Value, 0);
            Device.StartTimer(timer, () =>
            {
                 _randomSensingService.AllowSensing(true);
                _randomSensingService.StartRandom();
                return false;
            });

            // Return to main page to indicate confirmation
            await Shell.Current.GoToAsync("//Main");
        }
    }
}
