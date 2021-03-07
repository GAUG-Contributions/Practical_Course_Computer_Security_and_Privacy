using System;
using SensorFeedback.Models;
using SensorFeedback.Services;
using Tizen.Applications;
using Tizen.Applications.Messages;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SensorFeedback.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage, IDisposable
    { 
        // Used to deallocate resources for the services
        private bool _disposedValue = false;
        private RandomSensingService _randomSensingService;


        public MainPage()
        {
            InitializeComponent();
            _randomSensingService = RandomSensingService.GetInstance;
        }

        /*================================================================================*/
        /*=============================== BUTTON LISTENERS ===============================*/
        /*================================================================================*/

        protected override bool OnBackButtonPressed()
        {
            //Shell.Current.GoToAsync("//Main");
            return base.OnBackButtonPressed();
        }

        private void OnHealthButtonClicked(object sender, EventArgs args){
            bool bActive =_randomSensingService.SwitchStateHR();
            buttonHR.TextColor = bActive ? Color.Red : Color.White;
        }

        private void OnLocationButtonClicked(object sender, EventArgs args){
            bool bActive =_randomSensingService.SwitchStateLoc();
            buttonLocation.TextColor = bActive ? Color.Yellow : Color.White;
        }

        private void OnRandButtonClicked(object sender, EventArgs e)
        {
            bool bActive =_randomSensingService.SwitchRandomSensing();
            buttonRand.TextColor = bActive ? Color.Green : Color.White;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _randomSensingService.Dispose();
                }
                _randomSensingService = null;
                _disposedValue = true;
            }
        }

        ~MainPage()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}