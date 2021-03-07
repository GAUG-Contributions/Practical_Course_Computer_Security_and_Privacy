using SensorFeedback.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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
            _randomSensingService.SwitchRandomSensing();
            Device.StartTimer(new TimeSpan((int)StepperH.Value, (int)StepperM.Value, 0), () =>
            {
                _randomSensingService.SwitchRandomSensing();
                return false;
            });
        }

    }
}
