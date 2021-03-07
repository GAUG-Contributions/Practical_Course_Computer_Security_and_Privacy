using SensorFeedback.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace SensorFeedback.Views
{
    public partial class MonitoringSettingsPage : ContentPage
    {
        private RandomSensingService _randomSensingService;
        double _hour = 0;
        double _min = 0;

        /// <summary>
        /// Value of Hr Spinner
        /// </summary>
        public double Hour
        {
            get => _hour;
            set
            {
                if (_hour == value)
                {
                    return;
                }

                _hour = value;
            }
        }

        /// <summary>
        /// Value of Mm Spinner
        /// </summary>
        public double Minute
        {
            get => _min;
            set
            {
                if (_min == value)
                {
                    return;
                }

                _min = value;
            }
        }



        /// <summary>
        /// Constructor
        /// </summary>
        public MonitoringSettingsPage()
        {
            InitializeComponent();
            _randomSensingService = RandomSensingService.GetInstance;

            //// Set button event of SpinnerDefault
            //ButtonPressedExit = new Command(() =>
            //{
            //    Console.WriteLine($"Saved and Exit Value:{Value.ToString()}");
            //    Shell.Current.GoToAsync("//Main");
            //});

            //// Set button event of SpinnerTimer
            //TimerButtonPressedExit = new Command(() =>
            //{
            //    Console.WriteLine($"Saved and Exit Hour:{Hour}, Minute:{Minute}");
            //    Shell.Current.GoToAsync("//Main");
            //});
        }

        public void OnButtonSuspendClicked(object sender, EventArgs e)
        {
            // Get current time
            DateTime currentTime = DateTime.Now;

            // Time until which sensors will be disabled
            DateTime stopUntil = currentTime;
            stopUntil.AddHours(_hour);
            stopUntil.AddMinutes(_min);

            // Stop the randomization of services gathering sensor data
            _randomSensingService.SwitchRandomSensing();
        }
    }
}
