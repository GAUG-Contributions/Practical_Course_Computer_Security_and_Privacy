using System;
using System.Runtime.CompilerServices;
using TApplication = Tizen.Applications.Application;
using Tizen.Applications;
using Xamarin.Forms;
using Tizen.NUI.BaseComponents;
using SensorFeedbackWF.Services;
using System.Threading.Tasks;

namespace SensorFeedbackWF.Views
{
    public partial class MainPage : ContentPage
    {
        private DateTime _time;
        private string _timeString;
        private double animTimer = 1;
        private double timerDirection = 0.1;

        // For switching buttons on/off
        private bool bHealth = false;
        private bool bLocation = false;
        private bool bHealthAndLocation = false;

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

            // Subcribe to FeedbackService to trigger feedbacks when appropriate
            MessagingCenter.Subscribe<FeedbackService, FeedbackType>(this, "ShowRingFeedback", async (sender, arg) =>
            {
                await ShowRingFeedback(arg);
            });

            // Subcribe to FeedbackService to stop feedbacks when appropriate
            MessagingCenter.Subscribe<FeedbackService>(this, "StopRingFeedback", async (sender) =>
            {
                await StopRingFeedback();
            });
        }

        // Get or set time to be displayed.
        public string TimeString
        {
            get { return _timeString; }
            set { Set(ref _timeString, value); }
        }

        // Update time to be displayed.
        private void UpdateTime()
        {
            TimeString = _time.ToString("HH:mm:ss");
        }

        // Called at least once per second.
        private void OnTimeChanged(object sender, TimeEventArgs e)
        {
            _time = e.Time.UtcTimestamp;
            UpdateTime();
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
        private Task ShowRingFeedback(FeedbackType feedbackType)
        {
            progressBar.IsVisible = true;
            switch (feedbackType)
            {
                case FeedbackType.Health:
                    progressBar.Value = 1;
                    progressBar.BarColor = Color.Red;
                    progressBar.BackgroundColor = Color.Transparent;
                    break;
                case FeedbackType.Location:
                    progressBar.Value = 1;
                    progressBar.BarColor = Color.Yellow;
                    progressBar.BackgroundColor = Color.Transparent;
                    break;
                case FeedbackType.HealthAndLocation:
                    // Halve the bar to show both the health related bar and the location bar
                    progressBar.Value = 0.5;
                    progressBar.BarColor = Color.Yellow;
                    progressBar.BackgroundColor = Color.Red;
                    break;
            }
            //Subscribe to the tick event -> triggered every second
            (TApplication.Current as WatchApplication).TimeTick += AnimateRing;
            return Task.CompletedTask;
        }

        private Task StopRingFeedback()
        {
            progressBar.IsVisible = false;
            (TApplication.Current as WatchApplication).TimeTick -= AnimateRing;
            return Task.CompletedTask;
        }

        private void AnimateRing(object sender, TimeEventArgs e)
        {
            int counter = 0;
            // Continues firing every 100ms until it looped 10 times for a full second
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                progressBar.BarColor = new Color(progressBar.BarColor.R, progressBar.BarColor.G, progressBar.BarColor.B, this.animTimer);
                progressBar.BackgroundColor = new Color(progressBar.BackgroundColor.R, progressBar.BackgroundColor.G, progressBar.BackgroundColor.B, this.animTimer);

                if (this.animTimer >= 1 || this.animTimer <= 0)
                    timerDirection *= -1;
                this.animTimer += timerDirection;
                counter++;
                return counter != 10;
            });
        }

        /*================================================================================*/
        /*=============================== BUTTON LISTENERS ===============================*/
        /*================================================================================*/

        private void OnHealthButtonClicked(object sender, EventArgs args)
        {
            // First stop previous feedback
            StopRingFeedback();

            if (!bHealth)
            {
                // Activate HR Sensor
                ShowRingFeedback(FeedbackType.Health);
            }
            bHealth = !bHealth;
            bLocation = false;
            bHealthAndLocation = false;
        }

        private void OnLocationButtonClicked(object sender, EventArgs args)
        {
            // First stop previous feedback
            StopRingFeedback();

            if (!bLocation)
            {
                // Activate Location Sensor
                ShowRingFeedback(FeedbackType.Location);
            }
            bHealth = false;
            bLocation = !bLocation;
            bHealthAndLocation = false;
        }

        private void OnHealthAndLocationButtonClicked(object sender, EventArgs e)
        {
            // First stop previous feedback
            StopRingFeedback();

            if (!bHealthAndLocation)
            {
                // Activate HR & Location Sensors
                ShowRingFeedback(FeedbackType.HealthAndLocation);
            }
            bHealth = false;
            bLocation = false;
            bHealthAndLocation = !bHealthAndLocation;

        }
    }
}
