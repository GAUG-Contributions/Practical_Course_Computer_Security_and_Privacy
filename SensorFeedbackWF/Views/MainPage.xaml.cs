using System;
using System.Runtime.CompilerServices;
using TApplication = Tizen.Applications.Application;
using Tizen.Applications;
using Xamarin.Forms;
using SensorFeedbackWF.Services;
using System.Threading.Tasks;

namespace SensorFeedbackWF.Views
{
    public partial class MainPage : ContentPage
    {
        private DateTime _time;
        private string _timeString;

        // Ring blinking animation variables
        private double _animTimer = 1.0; // Should last one second
        private double _timerDirection = 0.1; // Is added to animTimer 10x per second so that the timer reaches 0 or 1 every second
        private bool _isAnimateRingActive = false;

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
            BindingContext = this;

            // Subscribe to the TimeTick event
            (TApplication.Current as WatchApplication).TimeTick += OnTimeChanged;
            (TApplication.Current as WatchApplication).AmbientTick += OnTimeChangedAmbiant;

            // Subcribe to FeedbackService to trigger feedbacks according to the FeedbackType
            MessagingCenter.Subscribe<FeedbackService, FeedbackType>(this, "UpdateRingFeedback", async (sender, feedback) =>
            {
                await UpdateRingFeedback(feedback);
            });
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
        private Task UpdateRingFeedback(FeedbackType feedback)
        {

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

    }
}
