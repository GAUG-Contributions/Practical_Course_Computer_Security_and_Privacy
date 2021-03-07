using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SensorFeedbackWF.Services;
using Tizen.Applications;
using Tizen.Applications.Messages;

using TApplication = Tizen.Applications.Application;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SensorFeedbackWF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage, IDisposable
    {
        private DateTime _time;
        private string _timeString;

        // Ring blinking animation variables
        private double _animTimer = 1.0; // Should last one second
        private double _timerDirection = 0.1; // Is added to animTimer 10x per second so that the timer reaches 0 or 1 every second
        private bool _isAnimateRingActive = false;

        // Services
        private MessagePortService _mpService;
        private RemotePort _remotePortService = null;

        // Communication
        private const string _localPort = "27072";
        private const string _remotePort = "27071";
        private const string _remoteAppId = "de.ugoe.SensorFeedback";
        private bool _trustedCommunication = true;

        // For future use 
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
            BindingContext = this;

            InitializeServices();

            // Subscribe to the TimeTick event
            (TApplication.Current as WatchApplication).TimeTick += OnTimeChanged;
            (TApplication.Current as WatchApplication).AmbientTick += OnTimeChangedAmbiant;

            // Subcribe to FeedbackService to trigger feedbacks according to the FeedbackType
            MessagingCenter.Subscribe<FeedbackService, string>(this, "ReceiveRingFeedback", async (sender, feedback) =>
            {
                await ReceiveRingFeedback(feedback);
            });

        }

        private void InitializeServices()
        {
            _mpService = new MessagePortService(_localPort, _trustedCommunication);
            _mpService.Open();
            _remotePortService = new RemotePort(_remoteAppId, _remotePort, _trustedCommunication);
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

        private bool Set<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
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
        private void UpdateRingFeedback(FeedbackType feedback)
        {
            // If an incorrect feedback type is received, do nothing
            // Or user doesn't want to be tracked
            if (feedback == FeedbackType.Error || !_areSensorsAllowed){
                return;
            }  

            // If both active
            if (feedback == FeedbackType.HealthAndLocation){

                progressBar.IsVisible = true;

                // Halve the bar to show both the health related bar and the location bar
                progressBar.Value = 0.5;
                progressBar.BarColor = Color.Yellow;
                progressBar.BackgroundColor = Color.Red;

            } else if (feedback == FeedbackType.NoFeedback)
            {
                progressBar.IsVisible = false;
                (TApplication.Current as WatchApplication).TimeTick -= AnimateRing;
                _isAnimateRingActive = false;

                // Don't remove, important for 'fast synchronization' aka 'shadowing desynchronization' :)
                // Just making the progressBar invisible doesn't remove
                // the color when there is no active service, at least on the emulator
                progressBar.BarColor = Color.Black;
                progressBar.BackgroundColor = Color.Black;
            }
            else
            { // only one active

                progressBar.IsVisible = true;
                progressBar.Value = 1;

                if (feedback == FeedbackType.Health)
                {
                    progressBar.BarColor = Color.Red;
                }
                if (feedback == FeedbackType.Location)
                {
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

        /*================================================================================*/
        /*============================== PORT COMMUNICATION ==============================*/
        /*================================================================================*/

        // This function is called inside MessagePortService.cs
        public Task ReceiveRingFeedback(String message){
            // In case the message format is incorrect, assign Error by default
            FeedbackType feedback = FeedbackType.Error;

            switch (message){
                case "NoFeedback": feedback = FeedbackType.NoFeedback; break;
                case "Health": feedback = FeedbackType.Health; break;
                case "Location": feedback = FeedbackType.Location; break;
                case "HealthAndLocation": feedback = FeedbackType.HealthAndLocation; break;
                default: break;
            }

            UpdateRingFeedback(feedback);
            return Task.CompletedTask;
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
                    _mpService.Dispose();
                    _remotePortService.Dispose();
                }
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
