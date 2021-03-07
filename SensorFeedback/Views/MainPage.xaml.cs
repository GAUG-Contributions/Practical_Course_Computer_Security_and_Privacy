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
        // Randomizer Functionality
        private Random _randomizer;
        // The number of random actions that the Rand button performs
        private const int _randomActionsToBePerformed = 200;
        private bool _isRandomActive = false;

        // Set this to true to get extra execution info of the random
        private const bool _printRandomRun = false;

        // Services
        private HeartRateMonitorService _hrmService;
        private LocationService _locService;
        private MessagePortService _mpService;
        private RemotePort _remotePortService = null;

        // Communication
        private const string _localPort = "27071";
        private const string _remotePort = "27072";
        private const string _remoteAppId = "de.ugoe.SensorFeedbackWF";
        private bool _trustedCommunication = true;

        // Services Activity Status
        private bool _hrmStatus = false;
        private bool _locStatus = false;
        private bool _areSensorsAllowed = true;

        // Used to deallocate resources for the services
        private bool _disposedValue = false;

        public enum FeedbackType
        {
            NoFeedback = 0,
            Health = 1,
            Location = 2,
            HealthAndLocation = 3,
            Error = 4 // Should never be the case
        }

        public MainPage()
        {
            InitializeComponent();
            InitializeServices();
        }

        private void InitializeServices(){
            _hrmService = new HeartRateMonitorService();
            _locService = new LocationService(Tizen.Location.LocationType.Hybrid);
            _randomizer = new Random();

            _mpService = new MessagePortService(_localPort, _trustedCommunication);
            _mpService.Open();
            _remotePortService = new RemotePort(_remoteAppId, _remotePort, _trustedCommunication);
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
            SwitchStateHR();
        }

        private void OnLocationButtonClicked(object sender, EventArgs args){
            SwitchStateLoc();
        }

        private void OnRandButtonClicked(object sender, EventArgs e)
        {
            // Switch random activity state
            // The user should be able to stop the random run before it's end
            _isRandomActive = !_isRandomActive;

            // If the user stops random mode before it ends
            if (!_isRandomActive){
                StopRandomFunction();
                return;
            }

            int previousAction = -1;
            int timeCounter = 10; // Used for faster responses to user interaction
            int randomActionsLeft = _randomActionsToBePerformed;
            buttonRand.TextColor = Color.Green;

            // Random action duration is 5 secs before changing to another action
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () => {
                if (randomActionsLeft >= 0 && timeCounter == 10 && _isRandomActive)
                {
                    previousAction = PerformOneRandomAction(previousAction);

                    // Set printRandomRun to true to run debug mode
                    if(_printRandomRun)
                        buttonRand.Text = randomActionsLeft.ToString() + " " + previousAction.ToString();

                    --randomActionsLeft;
                    timeCounter = 0;
                    if (randomActionsLeft < 0)
                    {
                        StopRandomFunction();
                        return false;
                    }
                }

                ++timeCounter;

                return true;
            });
        }

        /*================================================================================*/
        /*=============================== RANDOM MECHANISM ===============================*/
        /*================================================================================*/

        private int PerformOneRandomAction(int previousAction)
        {
            // Random action
            int randomAction = _randomizer.Next(0, 4);

            // Makes sure two actions of the same kind won't occur back to back
            while(previousAction == randomAction){
                randomAction = _randomizer.Next(0, 4);
            }

            switch (randomAction) {
                case 0: SwitchStateHR(); break; // Change HR State (on/of)
                case 1: SwitchStateLoc(); break; // Change Loc State (on/off)
                case 2: StartAllActions(); break; // Both turned on
                case 3: StopAllActions(); break; // Both turned off
                default: randomAction = -1; break;
            }

            return randomAction;
        }
        
        private void StartAllActions(){
            // If the user didn't allow sensors for some time skip
            if (!_areSensorsAllowed){
                StopRandomFunction();
                return;
            }

            if (!_hrmStatus){
                _hrmStatus = true;
                buttonHR.TextColor = Color.Red;
                _hrmService.Start();
            }

            if (!_locStatus){
                _locStatus = true;
                buttonLocation.TextColor = Color.Yellow;
                _locService.Start();
            }

            SendFeedbackToWF(FeedbackType.HealthAndLocation);
        }
        private void StopAllActions(){
            if (_hrmStatus){
                _hrmStatus = false;
                buttonHR.TextColor = Color.White;
                _hrmService.Stop();
            }

            if (_locStatus){
                _locStatus = false;
                buttonLocation.TextColor = Color.White;
                _locService.Stop();
            }

            SendFeedbackToWF(FeedbackType.NoFeedback);
        }
        private void SwitchStateHR(){
            // If the user didn't allow sensors for some time skip
            if (!_areSensorsAllowed){
                StopRandomFunction();
                return;
            }

            if (_hrmStatus){
                _hrmStatus = false;
                buttonHR.TextColor = Color.White;
                _hrmService.Stop();

                // The HRM is stopped but in order to not mess up the WF lightning of 
                // the other service we check if that service is active or not in order 
                // to send the correct feedback type to the WF
                if (_locStatus) SendFeedbackToWF(FeedbackType.Location);
                else SendFeedbackToWF(FeedbackType.NoFeedback);
            }
            else {
                _hrmStatus = true;
                buttonHR.TextColor = Color.Red;
                _hrmService.Start();

                // The HRM is started but in order to not mess up the WF lightning of 
                // the other service we check if that service is active or not in order 
                // to send the correct feedback type to the WF
                if (_locStatus) SendFeedbackToWF(FeedbackType.HealthAndLocation);
                else SendFeedbackToWF(FeedbackType.Health);
            }
        }
        private void SwitchStateLoc(){
            // If the user didn't allow sensors for some time skip
            if (!_areSensorsAllowed)
            {
                StopRandomFunction();
                return;
            }

            if (_locStatus){
                _locStatus = false;
                buttonLocation.TextColor = Color.White;
                _locService.Stop();

                // The Location is stopped but in order to not mess up the WF lightning of 
                // the other service we check if that service is active or not in order 
                // to send the correct feedback type to the WF
                if (_hrmStatus) SendFeedbackToWF(FeedbackType.Health);
                else SendFeedbackToWF(FeedbackType.NoFeedback);

            }
            else{
                _locStatus = true;
                buttonLocation.TextColor = Color.Yellow;
                _locService.Start();

                // The Location is started but in order to not mess up the WF lightning of 
                // the other service we check if that service is active or not in order 
                // to send the correct feedback type to the WF
                if (_hrmStatus) SendFeedbackToWF(FeedbackType.HealthAndLocation);
                else SendFeedbackToWF(FeedbackType.Location);
            }
        }
        
        private void StopRandomFunction(){
            buttonRand.TextColor = Color.White;
            // Set printRandomRun to true to run debug mode
            if (_printRandomRun) buttonRand.Text = "Rand";
            StopAllActions();
        }

        /*================================================================================*/
        /*============================== PORT COMMUNICATION ==============================*/
        /*================================================================================*/

        private void SendFeedbackToWF(FeedbackType feedback){
            // If the user didn't allow sensors for some time skip
            if (!_areSensorsAllowed){
                return;
            }

            // Check for User Settings to decide if there should also be vibration and/or sound
            DatabaseService ds = DatabaseService.GetInstance;
            UserSettings us = ds.LoadUserSettings();

            // If the remote app's port is listening
            if (_remotePort != null && _remotePortService.IsRunning()){
                var message = new Bundle();
                message.AddItem("visualFeedback", feedback.ToString());
                message.AddItem("vibrationFeedback", us.ActivateVibrationFeedback.ToString());
                message.AddItem("soundFeedback", us.ActivateSoundFeedback.ToString());
                _mpService.Send(message, _remotePortService.AppId, _remotePortService.PortName);
                message.Dispose();
            }
        }

        /*================================================================================*/
        /*=============================== DISPOSING RESOURCES ============================*/
        /*================================================================================*/
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _hrmService.Dispose();
                    _locService.Dispose();
                    _mpService.Dispose();
                    _remotePortService.Dispose();
                }

                _hrmService = null;
                _locService = null;
                _mpService = null;
                _remotePortService = null;
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