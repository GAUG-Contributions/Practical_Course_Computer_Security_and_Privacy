using System;
using System.Runtime.CompilerServices;
using System.Threading;
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

        private bool isAnimateRingActive = false;
        private int randomValue;

        // Sensors/Location Services
        private HeartRateMonitorService hrmService;
        private LocationService locService;

        // Sensors/Location Activity Status
        private bool hrmStatus = false;
        private bool locStatus = false;
        private bool randStatus = false;

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

            initializeServices();

            // Subscribe to the TimeTick event
            (TApplication.Current as WatchApplication).TimeTick += OnTimeChanged;

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

        private void initializeServices(){
            hrmService = new HeartRateMonitorService();
            locService = new LocationService(Tizen.Location.LocationType.Hybrid);
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
            randomValue = _time.Second * _time.Second * 11;
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
        private Task ShowRingFeedback()
        {
            progressBar.IsVisible = true;

            // If both active
            if(hrmStatus && locStatus){
                // Halve the bar to show both the health related bar and the location bar
                progressBar.Value = 0.5;
                progressBar.BarColor = Color.Yellow;
                progressBar.BackgroundColor = Color.Red;
            } else {
                progressBar.Value = 1;

                if (hrmStatus) progressBar.BarColor = Color.Red;
                if (locStatus) progressBar.BarColor = Color.Yellow;

                progressBar.BackgroundColor = Color.Transparent;
            }

            if(!isAnimateRingActive)
            {
                //Subscribe to the tick event -> triggered every second
                (TApplication.Current as WatchApplication).TimeTick += AnimateRing;
                isAnimateRingActive = true;
            }

            return Task.CompletedTask;
        }

        private Task StopRingFeedback()
        {
            // If both health and location are turned off
            if(!hrmStatus && !locStatus){
                progressBar.IsVisible = false;
                (TApplication.Current as WatchApplication).TimeTick -= AnimateRing;
                progressBar.BarColor = Color.Black;
                progressBar.BackgroundColor = Color.Black;
                isAnimateRingActive = false;
            }

            if (hrmStatus){
                progressBar.Value = 1;
                progressBar.BarColor = Color.Red;
                progressBar.BackgroundColor = Color.Transparent;
            }

            if (locStatus){
                progressBar.Value = 1;
                progressBar.BarColor = Color.Yellow;
                progressBar.BackgroundColor = Color.Transparent;
            }
            
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
            if (hrmStatus)
            {
                hrmStatus = false;
                hrmService.Stop();
                buttonHR.TextColor = Color.White;
            } else {
                hrmStatus = true;
                hrmService.Start();
                buttonHR.TextColor = Color.Red;
            }
        }

        private void OnLocationButtonClicked(object sender, EventArgs args)
        {
            if (locStatus)
            {
                locStatus = false;
                locService.Stop();
                buttonLocation.TextColor = Color.White;
            }
            else
            {
                locStatus = true;
                locService.Start();
                buttonLocation.TextColor = Color.Yellow;
            }
        }

        private void OnRandButtonClicked(object sender, EventArgs e)
        {
            // Starts with false and changes with every click
            randStatus = !randStatus;

            // Random turned off
            if (!randStatus){
                buttonRand.TextColor = Color.White;
                return;
            }

            buttonRand.TextColor = Color.Green;

            // Random action
            int randomAction = random(0, 100);
            //buttonRand.Text = randomAction.ToString();

            if(randomAction < 20){
                // Deactivate HRM sensor if active
                if (hrmStatus)
                {
                    hrmStatus = false;
                    hrmService.Stop();
                    buttonHR.TextColor = Color.White;
                }
            } else if (randomAction < 50){
                // Activate HRM sensor if not active
                if (!hrmStatus)
                {
                    hrmStatus = true;
                    hrmService.Start();
                    buttonHR.TextColor = Color.Red;
                }
            } else if (randomAction < 80){
                // Activate location tracking if not active
                if (!locStatus)
                {
                    locStatus = true;
                    locService.Start();
                    buttonLocation.TextColor = Color.Yellow;
                }
            } else {
                // Deactivate location tracking if active
                if (locStatus)
                {
                    locStatus = false;
                    locService.Stop();
                    buttonLocation.TextColor = Color.White;
                }
            }  
        }

        // Creating random number between min and max
        private int random(int min, int max){
            return (randomValue % max) + min;
        }

    }
}
