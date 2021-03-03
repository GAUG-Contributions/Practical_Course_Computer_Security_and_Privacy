using System;
using System.Runtime.CompilerServices;
using TApplication = Tizen.Applications.Application;
using Tizen.Applications;
using Xamarin.Forms;
using Tizen.NUI.BaseComponents;

namespace SensorFeedbackWF.Views
{
    public partial class MainPage : ContentPage
    {
        private DateTime _time;
        private string _timeString;
        private double animTimer = 1;
        private double timerDirection = 0.01;

        private enum FeedBackType
        {
            Health = 1,
            Location = 2,
            HealthAndLocation = 3,
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            ShowRingFeedback(FeedBackType.Location);

            // Subscribe to the TimeTick event
            (TApplication.Current as WatchApplication).TimeTick += OnTimeChanged;
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

        // Called when the plus button is clicked.
        private void ShowRingFeedback(FeedBackType feedbackType)
        {
            progressBar.IsVisible = true;
            switch (feedbackType)
            {
                case FeedBackType.Health:
                    progressBar.Value = 1;
                    progressBar.BarColor = Color.Red;
                    progressBar.BackgroundColor = Color.Transparent;
                    break;
                case FeedBackType.Location:
                    progressBar.Value = 1;
                    progressBar.BarColor = Color.Yellow;
                    progressBar.BackgroundColor = Color.Transparent;
                    break;
                case FeedBackType.HealthAndLocation:
                    // Halve the bar to show both the health related bar and the location bar
                    progressBar.Value = 0.5;
                    progressBar.BarColor = Color.Yellow;
                    progressBar.BackgroundColor = Color.Red;
                    break;
            }
            //Subscribe to the tick event -> triggered every second
            (TApplication.Current as WatchApplication).TimeTick += AnimateRing;
        }

        private void StopRingFeedback()
        {
            progressBar.IsVisible = false;
        }

        private void AnimateRing(object sender, TimeEventArgs e)
        {
            // If no feedback shown, no animation
            if (!progressBar.IsVisible) return;
            // Triggered
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                progressBar.BarColor = new Color(progressBar.BarColor.R, progressBar.BarColor.G, progressBar.BarColor.B, this.animTimer);
                progressBar.BackgroundColor = new Color(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, this.animTimer);

                if (this.animTimer >= 1 || this.animTimer <= 0)
                    timerDirection *= -1;
                this.animTimer += timerDirection;

                return progressBar.IsVisible;
            });
        }
    }
}
