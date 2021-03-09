using SensorFeedback.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tizen.Applications;
using Tizen.Applications.Messages;
using Xamarin.Forms;

namespace SensorFeedback.Services
{
    class RandomSensingService : IDisposable
    {
        // Main Services - Health, Location and Activity
        private HeartRateMonitorService _hrmService;
        private LocationService _locService;
        // private ActivityService _actService; - This is not implemented

        // Used to stop all sensors sensing if the user denied the access
        private bool _areSensorsAllowed = true;
        // Services Activity Status
        private bool _hrmStatus = false;
        private bool _locStatus = false;
        // although the activity service is not implemented
        // we would still like to have the blue feedback for activity
        private bool _actStatus = false;
        
        // Communication services
        private MessagePortService _mpService;
        private RemotePort _remotePortService = null;
        private const string _localPort = "27071";
        private const string _remotePort = "27072";
        private const string _remoteAppId = "de.ugoe.SensorFeedbackWF";
        private bool _trustedCommunication = true;

        // Randomizer Service
        private Random _randomizer;
        // Number of random actions that the Rand button performs
        private const int RANDOM_ACTIONS_TO_PERFORM = 200;
        private bool _isRandomActive = false;
        private const int NUM_TOTAL_SERVICES = 3;

        public enum ServiceType
        {
            NoService = 0,
            Health = 1,
            Location = 2,
            Activity = 3,
            Error = 4,
        }

        public enum ColorFeedback
        {
            NoFeedback = 0,
            Health = 1,
            Location = 2,
            Activity = 3,
            HealthAndLocation = 4,
            HealthAndActivity = 5,
            LocationAndActivity = 6,
            Error = 7 // Should never be the case
        }

        public enum VisualFeedback
        {
            NoFeedback = 0,
            Ring = 1,
            Icon = 2,
            Notification = 3,
            Error = 4 // Should never be the case
        }

        // For Sound and Vibration
        public enum OtherFeedback
        {
            NoFeedback = 0,
            Sound = 1,
            Vibration = 2,
            SoundAndVibration = 3,
            Error = 4 // Should never be the case
        }

        // Singleton
        private static RandomSensingService instance = null;
        public static RandomSensingService GetInstance
        {
            get
            {
                if (instance == null)
                    instance = new RandomSensingService();
                return instance;
            }
        }

        // Used to track if the resources allocated were disposed
        private bool _disposedValue = false;

        RandomSensingService()
        {
            InitializeServices();
        }

        private void InitializeServices()
        {
            _randomizer = new Random();
            _hrmService = new HeartRateMonitorService();
            _locService = new LocationService(Tizen.Location.LocationType.Hybrid);
            _mpService = new MessagePortService(_localPort, _trustedCommunication);
            //_actService = new ... This is not implemented
            _mpService.Open();
            _remotePortService = new RemotePort(_remoteAppId, _remotePort, _trustedCommunication);
        }

        // Allows or Forbids sensor's sensing
        public void AllowSensing(bool value) {
            _areSensorsAllowed = value;

            // If services are suspended, set random functionality to false
            // Send feedback to the WF
            if (!_areSensorsAllowed){
                StopRandom();
                StopAllServices();
                SendFeedbackToWF();
            }
        }
        public bool AreSensonrsAllowed(){
            return _areSensorsAllowed;
        }

        // Starts a service if not already running
        public bool StartService(ServiceType service){
            if (service == ServiceType.Health && !_hrmStatus){
                _hrmStatus = true;
                _hrmService.Start();
                return true;
            }

            if (service == ServiceType.Location && !_locStatus)
            {
                _locStatus = true;
                _locService.Start();
                return true;
            }

            if (service == ServiceType.Activity && !_actStatus)
            {
                _actStatus = true;
                // _actService.Start(); - This is not implemented
                return true;
            }

            return false;
        }

        // Stops a service if running
        public bool StopService(ServiceType service){
            if (service == ServiceType.Health && _hrmStatus){
                _hrmStatus = false;
                _hrmService.Stop();
                return true;
            }

            if (service == ServiceType.Location && _locStatus){
                _locStatus = false;
                _locService.Stop();
                return true;
            }

            if (service == ServiceType.Activity && _actStatus){
                _actStatus = false;
                // _actService.Stop(); - This is not implemented
                return true;
            }

            return false;
        }

        public void StopAllServices()
        {
            StopService(ServiceType.Health);
            StopService(ServiceType.Location);
            StopService(ServiceType.Activity);
        }

        // Checks if a service is active or not
        public bool isServiceActive(ServiceType service){
            if (service == ServiceType.Health) return _hrmStatus;
            if (service == ServiceType.Location) return _locStatus;
            if (service == ServiceType.Activity) return _actStatus;

            return false;
        }

        // Returns the amount of active services
        public int NumOfActiveServices(){
            int result = 0;

            if (_hrmStatus) ++result;
            if (_locStatus) ++result;
            if (_actStatus) ++result;

            return result;
        }

        // Starts one random service if there aren't 
        // already running enough services - in this project 2 is max
        public ServiceType StartOneRandomService(){

            // Default service
            ServiceType service = ServiceType.NoService;

            // Check the number of currently running services
            // If there are 2 or more services active - return
            if(NumOfActiveServices() >= NUM_TOTAL_SERVICES-1){
                return service;
            }

            // True when a non-active service is found
            bool nonActiveFound = false;

            // Loop till an inactive service is found
            do{
                // Generate random number between 1 and NUM_TOTAL_SERVICES inclusive
                // There are currently 3 different sensors
                int randomNumber = _randomizer.Next(1, NUM_TOTAL_SERVICES + 1);

                // Select a service according to it's Enum value
                service = (ServiceType)randomNumber;

                // If the selected service is not active
                if (!isServiceActive(service)){

                    // Start the selected service
                    StartService(service);

                    nonActiveFound = true;
                }

            } while (!nonActiveFound);

            // Return the started service
            return service;
        }

        // Stops one random service if there are any running
        public ServiceType StopOneRandomService(){
            // Default service
            ServiceType service = ServiceType.NoService;

            // No active services available
            if (NumOfActiveServices() <= 0) return service;

            // True when an active service is found
            bool activeFound = false;

            // Loop till an active service is found
            do
            {
                // Generate random number between 1 and NUM_TOTAL_SERVICES inclusive
                // There are currently 3 different sensors
                int randomNumber = _randomizer.Next(1, NUM_TOTAL_SERVICES + 1);

                // Select a service according to it's Enum value
                service = (ServiceType)randomNumber;

                // If the selected service is active
                if (isServiceActive(service))
                {
                    // Stop the selected service
                    StopService(service);
                    activeFound = true;
                }

            } while (!activeFound);

            return service;
        }

        // Starts maximum amount of possible services at the same time
        // In this project 2 is max
        public void StartMaxRandomServices(){
            while(NumOfActiveServices() < NUM_TOTAL_SERVICES-1){
                StartOneRandomService();
            }
        }

        // Performs one smart random action depending on the previous action
        private int PerformOneRandomAction(int previousAction)
        {
            int randomAction = -1;

            // Generate a random action
            randomAction = _randomizer.Next(0, 4);

            switch (previousAction){
                // If the previous action was to start one random service 
                case 0:
                    // and currently there are maximum amount of services running:
                    // Randomly generate a new (case: 2) or (case: 3) action
                    if (NumOfActiveServices() <= NUM_TOTAL_SERVICES-1)
                        randomAction = _randomizer.Next(2, 4);
                    break;

                // If the previous action was to start max amount of services
                // it makes no sense to start another service (case: 0) or go for the 
                // same action (case: 1) again, so randomly generate a (case: 2) or (case: 3) action
                case 1: randomAction = _randomizer.Next(2, 4); break;

                // If the previous action was to stop one random service
                case 2:
                    // and currently there aren't any running services:
                    // Randomly generate a new (case: 0) or (case: 1) action
                    if (NumOfActiveServices() <= 0)
                        randomAction = _randomizer.Next(0, 2);
                    break;

                // If the previous action was to stop all available services
                // it makes no sense to stop any service (case: 2) or go for the 
                // same action (case: 3) again, so randomly generate a (case: 0) or (case: 1) action
                case 3: randomAction = _randomizer.Next(0, 2); break;

                // If there is no previous action or on action error
                case -1: randomAction = _randomizer.Next(0, 4); break;
            }

            switch (randomAction)
            {
                // Starts 1 random service
                case 0: StartOneRandomService(); break; 
                // Starts max amount (in this project 2) of services 
                case 1: StartMaxRandomServices(); break;
                // Stops one of the currently running services
                case 2: StopOneRandomService(); break;
                // Stops all services
                case 3: StopAllServices(); break; 
                default: randomAction = -1; break;
            }

            return randomAction;
        }

        public void StartRandom(){
            _isRandomActive = true;
            PerformRandomActions();
        }
        public void StopRandom(){
            _isRandomActive = false;
            PerformRandomActions();
        }
        public bool IsRandomActive(){
            return _isRandomActive;
        }

        // Coninuously performs random actions - RANDOM_ACTIONS_TO_PERFORM times
        private bool PerformRandomActions(){
            // If the sensing is disabled
            if (!AreSensonrsAllowed())
            {
                StopAllServices();
                SendFeedbackToWF();
                return false;
            }

            // If the user stops random mode before it ends
            if (!IsRandomActive())
            {
                StopAllServices();
                SendFeedbackToWF();
                return false;
            }

            int previousAction = -1;
            int timeCounter = 10; // Used for faster responses to user interaction
            int randomActionsLeft = RANDOM_ACTIONS_TO_PERFORM;

            // Random action duration is 5 secs before changing to another action
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () => {
                if (randomActionsLeft >= 0 && timeCounter == 10 && IsRandomActive() && AreSensonrsAllowed())
                {
                    previousAction = PerformOneRandomAction(previousAction);
                    SendFeedbackToWF();

                    --randomActionsLeft;
                    timeCounter = 0;
                    if (randomActionsLeft < 0)
                    {
                        StopAllServices();
                        SendFeedbackToWF();
                        return false;
                    }
                }

                // If the sensors are suspended, stop
                if (!AreSensonrsAllowed()){
                    return false;
                }

                ++timeCounter;

                return true;
            });

            return _isRandomActive;
        }

        // Generates the appropriate color feadback depending on active services
        private ColorFeedback generateColorFeedback(){
            ColorFeedback color = ColorFeedback.NoFeedback;
            int servicesNum = NumOfActiveServices();

            if (servicesNum <= 0) return color;

            // If there is one active service, assign the color appropriately
            if(servicesNum == 1){
                if (isServiceActive(ServiceType.Health)) color = ColorFeedback.Health;
                if (isServiceActive(ServiceType.Location)) color = ColorFeedback.Location;
                if (isServiceActive(ServiceType.Activity)) color = ColorFeedback.Activity;
            }

            // If there are two active services, check which service is not active and assign the color appropriately
            if(servicesNum == 2){
                if (!isServiceActive(ServiceType.Health)) color = ColorFeedback.LocationAndActivity;
                if (!isServiceActive(ServiceType.Location)) color = ColorFeedback.HealthAndActivity;
                if (!isServiceActive(ServiceType.Activity)) color = ColorFeedback.HealthAndLocation;
            }

            return color;
        }

        // Data sent to the WF app
        private void SendFeedbackToWF()
        {
            // If the user didn't allow sensors for some time skip
            if (!_areSensorsAllowed) return;

            // If the remote app's port is listening
            if (_remotePort != null && _remotePortService.IsRunning())
            {
                // Generates color feedback according to the current activity state of services
                ColorFeedback color = generateColorFeedback();

                // Check for User Settings to decide if there should also be vibration and/or sound
                DatabaseService ds = DatabaseService.GetInstance;
                UserSettings us = ds.LoadUserSettings();

                var message = new Bundle();
                message.AddItem("colorFeedback", color.ToString());
                // Insert here the appropriate DB access to choose the Ring, Icon and Notific - Should be done
                message.AddItem("visualFeedback", "Ring");
                message.AddItem("vibrationFeedback", us.ActivateVibrationFeedback.ToString());
                message.AddItem("soundFeedback", us.ActivateSoundFeedback.ToString());
                // message.AddItem("vibrationFeedback", "false"); - For debug
                // message.AddItem("soundFeedback", "false"); - For debug 
                _mpService.Send(message, _remotePortService.AppId, _remotePortService.PortName);
                
                message.Dispose();

            } else {
                MessagingCenter.Send(this, "printMe", "PORT_ERROR");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _hrmService.Dispose();
                    _locService.Dispose();
                    // _actService.Dispose(); - this is not implemented
                    _mpService.Dispose();
                    _remotePortService.Dispose();
                }

                _hrmService = null;
                _locService = null;
                // _actService = null; - this is not implemented
                _mpService = null;
                _remotePortService = null;
                _disposedValue = true;
            }
        }

        ~RandomSensingService()
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
