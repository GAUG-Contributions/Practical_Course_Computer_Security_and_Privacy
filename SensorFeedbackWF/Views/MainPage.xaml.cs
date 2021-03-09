using SensorFeedbackWF.Services;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tizen.Applications;
using Tizen.Applications.Messages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TApplication = Tizen.Applications.Application;

namespace SensorFeedbackWF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage, IDisposable
    {
        // Get or set time to be displayed.
        private DateTime _time;
        private string _timeString;
        public string TimeString
        {
            get { return _timeString; }
            set { Set(ref _timeString, value); }
        }

        // Used to stop all sensors sensing if the user denied the access
        private bool _areSensorsAllowed = true;

        // Ring blinking animation variables
        private double _animTimer = 1.0; // Should last one second
        private double _timerDirection = 0.01; // Is added to animTimer 10x per second so that the timer reaches 0 or 1 every second
        private bool _isAnimateRingActive = false;

        // Communication services
        private MessagePortService _mpService;
        private RemotePort _remotePortService = null;
        private const string _localPort = "27072";
        // private const string _remotePort = "27071";
        // private const string _remoteAppId = "de.ugoe.SensorFeedback";
        private bool _trustedCommunication = true;

        // Used to track if the resources allocated were disposed
        private bool _disposedValue = false;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            InitializeServices();

            // Subscribe to the TimeTick event
            (TApplication.Current as WatchApplication).TimeTick += OnTimeChanged;
            (TApplication.Current as WatchApplication).AmbientTick += OnTimeChangedAmbiant;

            // Used to receive ring color settings from the FeedbackService
            MessagingCenter.Subscribe<FeedbackService, Bundle>(this, "ReceiveRingSettings", async (sender, message) =>
            {
                await ReceiveRingSettings(message);
            });

            // Used to receive icon color settings from the FeedbackService
            MessagingCenter.Subscribe<FeedbackService, Bundle>(this, "ReceiveIconSettings", async (sender, message) =>
            {
                await ReceiveIconSettings(message);
            });

            // Used to debug - don't remove before the final version
            MessagingCenter.Subscribe<FeedbackService, String>(this, "printMe", async (sender, str) =>
            {
                await printMe(str);
            });

            // Used to debug - don't remove before the final version
            MessagingCenter.Subscribe<MessagePortService, Color>(this, "printMe2", async (sender, col) =>
            {
                await printMe2(col);
            });
        }

        // Used to debug - don't remove before the final version
        private Task printMe(string str)
        {
            DebugLabel.Text = str;

            return Task.CompletedTask;
        }

        // Used to debug - don't remove before the final version
        private Task printMe2(Color color)
        {
            DebugLabel.TextColor = color;

            return Task.CompletedTask;
        }

        private void InitializeServices()
        {
            _mpService = new MessagePortService(_localPort, _trustedCommunication);
            _mpService.Open();
            // Not needed for now because we don't send anything from WF to the Main App
            //_remotePortService = new RemotePort(_remoteAppId, _remotePort, _trustedCommunication);
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
        private void UpdateRingFeedback(bool isVisible, double value, Color bar, Color background)
        {
            // If the user doesn't want to be tracked - return
            if (!_areSensorsAllowed) return;

            progressBar.IsVisible = isVisible;
            progressBar.Value = value;
            progressBar.BarColor = bar;
            progressBar.BackgroundColor = background;

            // If the ring is visible, subscribe to the tick event if not already subscribed
            if (isVisible)
            {
                if (!_isAnimateRingActive)
                {
                    _isAnimateRingActive = true;
                    // Subscribe to the tick event -> triggered every second
                    (TApplication.Current as WatchApplication).TimeTick += AnimateRing;
                }
            } // If the ring is not visible, remove the subscription is there is one
            else
            {
                if (_isAnimateRingActive)
                {
                    _isAnimateRingActive = false;
                    // Remove the subscription
                    (TApplication.Current as WatchApplication).TimeTick -= AnimateRing;
                }
            }
        }

        private void UpdateIconFeedback(bool bHealthIconActive, bool bLocationIconActive, bool bActivityIconActive)
        {
            // If the user doesn't want to be tracked - turn off
            if (!_areSensorsAllowed)
            {
                healthIcon.IsVisible = false;
                locationIcon.IsVisible = false;
                activityIcon.IsVisible = false;
            }
            healthIcon.IsVisible = bHealthIconActive;
            locationIcon.IsVisible = bLocationIconActive;
            activityIcon.IsVisible = bActivityIconActive;
        }

        private void AnimateRing(object sender, TimeEventArgs e)
        {
            // Continues firing every 100ms until it looped 10 times for a full second
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                // Interact on the UI thread
                progressBar.BarColor = new Color(progressBar.BarColor.R, progressBar.BarColor.G, progressBar.BarColor.B, this._animTimer);
                progressBar.BackgroundColor = new Color(progressBar.BackgroundColor.R, progressBar.BackgroundColor.G, progressBar.BackgroundColor.B, this._animTimer);

                if (this._animTimer >= 1 || this._animTimer <= 0)
                    this._timerDirection *= -1;
                this._animTimer += _timerDirection;
                return this._animTimer != 0 && this._animTimer != 1;
            });
        }

        /*================================================================================*/
        /*============================== PORT COMMUNICATION ==============================*/
        /*================================================================================*/

        // This function is called inside MessagePortService.cs
        public Task ReceiveRingSettings(Bundle message)
        {
            if (message == null) throw new ArgumentNullException("message");

            // Stop other potential visual feedback
            UpdateIconFeedback(false, false, false);

            // Get separated strings
            string isVisibleStr = message.GetItem("isVisible").ToString();
            string valueStr = message.GetItem("barValue").ToString();
            string colorStr = message.GetItem("barColor").ToString();
            string backgroundStr = message.GetItem("backgroundColor").ToString();

                // Set default values
                bool parsingSuccessful = true;

                // Try to parse booleans
                if (!bool.TryParse(isVisibleStr, out bool isVisible))
                parsingSuccessful = false;

            if (!double.TryParse(valueStr, out double barValue))
                parsingSuccessful = false;

            // Parse colours
            Color bar = Color.FromHex(colorStr);
            Color background = Color.FromHex(backgroundStr);

            // If the parsing was successful, update ring feedback
            if (parsingSuccessful)
            {
                UpdateRingFeedback(isVisible, barValue, bar, background);
            }
            else
            {
                DebugLabel.Text = "FailedRingSettings";
                DebugLabel.TextColor = Color.Red;
            }

            return Task.CompletedTask;
        }

        public Task ReceiveIconSettings(Bundle message)
        {
            if (message == null) throw new ArgumentNullException("message");

            // Stop other potential visual feedback
            UpdateRingFeedback(false, 0, Color.Transparent, Color.Transparent);

            _ = bool.TryParse(message.GetItem("bHealthIconActive").ToString(), out bool bHealthIconActive);
            _ = bool.TryParse(message.GetItem("bLocationIconActive").ToString(), out bool bLocationIconActive);
            _ = bool.TryParse(message.GetItem("bActivityIconActive").ToString(), out bool bActivityIconActive);

            UpdateIconFeedback(bHealthIconActive, bLocationIconActive, bActivityIconActive);

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
