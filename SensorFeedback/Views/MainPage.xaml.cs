using SensorFeedbackWF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private const int _randomActionsToBePerformed = 10;
        private bool _isRandomActive = false;

        // Set this to true to get extra execution info of the random
        private const bool _printRandomRun = false;

        // Sensors/Location Services
        private HeartRateMonitorService _hrmService;
        private LocationService _locService;

        // Sensors/Location Activity Status
        private bool _hrmStatus = false;
        private bool _locStatus = false;

        // Used to deallocate resources for the services
        private bool _disposedValue = false;

        public enum FeedbackType
        {
            NoFeedback = 0,
            Health = 1,
            Location = 2,
            HealthAndLocation = 3,
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
        }

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

            //progressBar.BarColor = Color.Black;
            //progressBar.BackgroundColor = Color.Black;

        }

        private void SwitchStateHR(){
            if (_hrmStatus){
                _hrmStatus = false;
                buttonHR.TextColor = Color.White;
                _hrmService.Stop();
            }
            else{
                _hrmStatus = true;
                buttonHR.TextColor = Color.Red;
                _hrmService.Start();
            }
        }

        private void SwitchStateLoc(){
            if (_locStatus){
                _locStatus = false;
                buttonLocation.TextColor = Color.White;
                _locService.Stop();
            }
            else{
                _locStatus = true;
                buttonLocation.TextColor = Color.Yellow;
                _locService.Start();
            }
        }

        private void StopRandomFunction(){
            buttonRand.TextColor = Color.White;
            // Set printRandomRun to true to run debug mode
            if (_printRandomRun) buttonRand.Text = "Rand";
            StopAllActions();
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
                }

                _hrmService = null;
                _locService = null;
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