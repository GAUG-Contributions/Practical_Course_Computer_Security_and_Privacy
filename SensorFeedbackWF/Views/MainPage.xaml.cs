using System;
using System.Runtime.CompilerServices;
using System.Threading;
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
        private double animTimer = 1.0; // Should last one second
        private double timerDirection = 0.1; // Is added to animTimer 10x per second so that the timer reaches 0 or 1 every second
        private bool isAnimateRingActive = false;

        // Randomizer Functionality
        private Random randomizer;
        // The number of random actions that the Rand button performs
        private const int randomActionsToBePerformed = 10;
        private bool isRandomActive = false;

        // Set this to true to get extra execution info of the random
        private const bool printRandomRun = false;

        // Sensors/Location Services
        private HeartRateMonitorService hrmService;
        private LocationService locService;

        // Sensors/Location Activity Status
        private bool hrmStatus = false;
        private bool locStatus = false;

        // Used to deallocate resources for the services
        private bool disposedValue = false;

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

            initializeServices();

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
            randomizer = new Random();
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
        private Task ShowRingFeedback()
        {
            progressBar.IsVisible = true;

            // If both active
            if (hrmStatus && locStatus){
                // Halve the bar to show both the health related bar and the location bar
                progressBar.Value = 0.5;
                progressBar.BarColor = Color.Yellow;
                progressBar.BackgroundColor = Color.Red;

            } 
            else { // only one active

                progressBar.Value = 1;

                if (hrmStatus){
                    progressBar.BarColor = Color.Red;
                }
                if (locStatus){
                    progressBar.BarColor = Color.Yellow;
                }
                
                progressBar.BackgroundColor = Color.Transparent;
            }

            // This check is performed to make sure that the AnimateRing won't get activated 
            // more than once. Activating more than once leads to faster blinking.
            if (!isAnimateRingActive)
            {
                isAnimateRingActive = true;
                //Subscribe to the tick event -> triggered every second
                (TApplication.Current as WatchApplication).TimeTick += AnimateRing;

            }

            return Task.CompletedTask;
        }

        private Task StopRingFeedback()
        {
            // If both health and location are turned off
            if(!hrmStatus && !locStatus){
                progressBar.IsVisible = false;
                (TApplication.Current as WatchApplication).TimeTick -= AnimateRing;
                isAnimateRingActive = false;

                // Don't remove, important for 'fast synchronization' aka 'shadowing desynchronization' :)
                // Just making the progressBar invisible doesn't remove
                // the color when there is no active service, at least on the emulator
                progressBar.BarColor = Color.Black;
                progressBar.BackgroundColor = Color.Black;
                
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
            // Continues firing every 100ms until it looped 10 times for a full second
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                progressBar.BarColor = new Color(progressBar.BarColor.R, progressBar.BarColor.G, progressBar.BarColor.B, this.animTimer);
                progressBar.BackgroundColor = new Color(progressBar.BackgroundColor.R, progressBar.BackgroundColor.G, progressBar.BackgroundColor.B, this.animTimer);

                if (this.animTimer >= 1 || this.animTimer <= 0)
                    this.timerDirection *= -1;
                this.animTimer += timerDirection;
                return this.animTimer != 0 && this.animTimer != 1;
            });
        }

        /*================================================================================*/
        /*=============================== BUTTON LISTENERS ===============================*/
        /*================================================================================*/

        private void OnHealthButtonClicked(object sender, EventArgs args){
            switchStateHR();
        }

        private void OnLocationButtonClicked(object sender, EventArgs args){
            switchStateLoc();
        }

        private void OnRandButtonClicked(object sender, EventArgs e)
        {
            // Switch random activity state
            // The user should be able to stop the random run before it's end
            isRandomActive = !isRandomActive;

            // If the user stops random mode before it ends
            if (!isRandomActive){
                stopRandomFunction();
                return;
            }

            int previousAction = -1;
            int timeCounter = 10; // Used for faster responses to user interaction
            int randomActionsLeft = randomActionsToBePerformed;
            buttonRand.TextColor = Color.Green;

            // Random action duration is 5 secs before changing to another action
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () => {  
                if (randomActionsLeft >= 0 && timeCounter == 10)
                {
                    previousAction = performOneRandomAction(previousAction);

                    // Set printRandomRun to true to run debug mode
                    if(printRandomRun)
                        buttonRand.Text = randomActionsLeft.ToString() + " " + previousAction.ToString();

                    --randomActionsLeft;
                    timeCounter = 0;
                    if (randomActionsLeft < 0)
                    {
                        stopRandomFunction();
                        return false;
                    }
                }

                ++timeCounter;

                return true;
            });
        }
        
        private int performOneRandomAction(int previousAction)
        {
            // Random action
            int randomAction = randomizer.Next(0, 4);

            // Makes sure two actions of the same kind won't occur back to back
            while(previousAction == randomAction){
                randomAction = randomizer.Next(0, 4);
            }

            switch (randomAction) {
                case 0: switchStateHR(); break; // Change HR State (on/of)
                case 1: switchStateLoc(); break; // Change Loc State (on/off)
                case 2: startAllActions(); break; // Both turned on
                case 3: stopAllActions(); break; // Both turned off
                default: randomAction = -1; break;
            }

            return randomAction;
        }
        
        private void startAllActions(){
            if (!hrmStatus){
                hrmStatus = true;
                buttonHR.TextColor = Color.Red;
                hrmService.Start();
            }

            if (!locStatus){
                locStatus = true;
                buttonLocation.TextColor = Color.Yellow;
                locService.Start();
            }
        }
        private void stopAllActions(){
            if (hrmStatus){
                hrmStatus = false;
                buttonHR.TextColor = Color.White;
                hrmService.Stop();
            }

            if (locStatus){
                locStatus = false;
                buttonLocation.TextColor = Color.White;
                locService.Stop();
            }

            progressBar.BarColor = Color.Black;
            progressBar.BackgroundColor = Color.Black;

        }
        private void switchStateHR(){
            if (hrmStatus){
                hrmStatus = false;
                buttonHR.TextColor = Color.White;
                hrmService.Stop();
            }
            else{
                hrmStatus = true;
                buttonHR.TextColor = Color.Red;
                hrmService.Start();
            }
        }
        private void switchStateLoc(){
            if (locStatus){
                locStatus = false;
                buttonLocation.TextColor = Color.White;
                locService.Stop();
            }
            else{
                locStatus = true;
                buttonLocation.TextColor = Color.Yellow;
                locService.Start();
            }
        }
        private void stopRandomFunction(){
            buttonRand.TextColor = Color.White;
            // Set printRandomRun to true to run debug mode
            if (printRandomRun) buttonRand.Text = "Rand";
            stopAllActions();
        }

        /*================================================================================*/
        /*=============================== DISPOSING RESOURCES ============================*/
        /*================================================================================*/
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    hrmService.Dispose();
                    locService.Dispose();
                }

                hrmService = null;
                locService = null;
                disposedValue = true;
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
