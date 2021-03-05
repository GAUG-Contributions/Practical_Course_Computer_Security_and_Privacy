using System;
using System.Runtime.CompilerServices;
using TApplication = Tizen.Applications.Application;
using Tizen.Applications;
using Xamarin.Forms;
using SensorFeedbackWF.Services;
using System.Threading.Tasks;

namespace SensorFeedbackWF.Views
{
    public partial class MainPage : ContentPage, IDisposable
    {
        private DateTime _time;
        private string _timeString;

        // Ring blinking animation variables
        private double _animTimer = 1.0; // Should last one second
        private double _timerDirection = 0.1; // Is added to animTimer 10x per second so that the timer reaches 0 or 1 every second
        private bool _isAnimateRingActive = false;

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
            Health = 1,
            Location = 2,
            HealthAndLocation = 3,
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            // Subscribe to the TimeTick event
            (TApplication.Current as WatchApplication).TimeTick += OnTimeChanged;
            (TApplication.Current as WatchApplication).AmbientTick += OnTimeChangedAmbiant;
            (TApplication.Current as WatchApplication).AmbientChanged += ToggleButtonsVisibility;

            InitializeServices();

            // Subcribe to FeedbackService to trigger feedbacks when appropriate
            MessagingCenter.Subscribe<FeedbackService>(this, "ShowRingFeedback", async (sender) =>
            {
                await ShowRingFeedback();
            });

            // Subcribe to FeedbackService to stop feedbacks when appropriate
            MessagingCenter.Subscribe<FeedbackService>(this, "StopRingFeedback", async (sender) =>
            {
                await StopRingFeedback();
            });

        }

        private void ToggleButtonsVisibility(object sender, AmbientEventArgs e)
        {
            buttonHR.IsVisible = !e.Enabled;
            buttonLocation.IsVisible = !e.Enabled;
            buttonRand.IsVisible = !e.Enabled;
        }

        private void InitializeServices(){
            _hrmService = new HeartRateMonitorService();
            _locService = new LocationService(Tizen.Location.LocationType.Hybrid);
            _randomizer = new Random();
        }

        // Get or set time to be displayed.
        public string TimeString
        {
            get { return _timeString; }
            set { Set(ref _timeString, value); }
        }

        // Update time to be displayed.
        private void OnTimeChangedAmbiant(object sender, TimeEventArgs e)
        {
            _time = e.Time.UtcTimestamp;
            TimeString = _time.ToString("HH:mm");
        }

        // Called at least once per second.
        private void OnTimeChanged(object sender, TimeEventArgs e)
        {
            _time = e.Time.UtcTimestamp;
            TimeString = _time.ToString("HH:mm:ss");
        }

        private bool Set<T>(ref T property, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(property, value))
            {
                return false;
            }

            property = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        /*================================================================================*/
        /*=============================== FEEDBACK METHODS ===============================*/
        /*================================================================================*/

        // Displays an outer colored ring on the Watch Face. Color depends on sensor type.
        private Task ShowRingFeedback()
        {
            progressBar.IsVisible = true;

            // If both active
            if (_hrmStatus && _locStatus){
                // Halve the bar to show both the health related bar and the location bar
                progressBar.Value = 0.5;
                progressBar.BarColor = Color.Yellow;
                progressBar.BackgroundColor = Color.Red;

            } 
            else { // only one active

                progressBar.Value = 1;

                if (_hrmStatus){
                    progressBar.BarColor = Color.Red;
                }
                if (_locStatus){
                    progressBar.BarColor = Color.Yellow;
                }
                
                progressBar.BackgroundColor = Color.Transparent;
            }

            // This check is performed to make sure that the AnimateRing won't get activated 
            // more than once. Activating more than once leads to faster blinking.
            if (!_isAnimateRingActive)
            {
                _isAnimateRingActive = true;
                //Subscribe to the tick event -> triggered every second
                (TApplication.Current as WatchApplication).TimeTick += AnimateRing;

            }

            return Task.CompletedTask;
        }

        private Task StopRingFeedback()
        {
            // If both health and location are turned off
            if(!_hrmStatus && !_locStatus){
                progressBar.IsVisible = false;
                (TApplication.Current as WatchApplication).TimeTick -= AnimateRing;
                _isAnimateRingActive = false;

                // Don't remove, important for 'fast synchronization' aka 'shadowing desynchronization' :)
                // Just making the progressBar invisible doesn't remove
                // the color when there is no active service, at least on the emulator
                progressBar.BarColor = Color.Black;
                progressBar.BackgroundColor = Color.Black;
                
            }

            if (_hrmStatus){
                progressBar.Value = 1;
                progressBar.BarColor = Color.Red;
                progressBar.BackgroundColor = Color.Transparent;
            } 

            if (_locStatus){
                progressBar.Value = 1;
                progressBar.BarColor = Color.Yellow;
                progressBar.BackgroundColor = Color.Transparent;
            }
            
            return Task.CompletedTask;
        }

        private void AnimateRing(object sender, TimeEventArgs e)
        {
            // Continues firing every 100ms until it looped 10 times for a full second
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                progressBar.BarColor = new Color(progressBar.BarColor.R, progressBar.BarColor.G, progressBar.BarColor.B, this._animTimer);
                progressBar.BackgroundColor = new Color(progressBar.BackgroundColor.R, progressBar.BackgroundColor.G, progressBar.BackgroundColor.B, this._animTimer);

                if (this._animTimer >= 1 || this._animTimer <= 0)
                    this._timerDirection *= -1;
                this._animTimer += _timerDirection;
                return this._animTimer != 0 && this._animTimer != 1;
            });
        }

        /*================================================================================*/
        /*=============================== BUTTON LISTENERS ===============================*/
        /*================================================================================*/

        protected override bool OnBackButtonPressed()
        {
            Shell.Current.GoToAsync("//Main");
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

            progressBar.BarColor = Color.Black;
            progressBar.BackgroundColor = Color.Black;

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
