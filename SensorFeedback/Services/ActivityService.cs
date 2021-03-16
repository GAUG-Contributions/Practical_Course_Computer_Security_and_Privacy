using System;
using Tizen.Applications;
using Tizen.Sensor;

namespace SensorFeedback.Services
{
    class ActivityService : IDisposable
    {
        private Pedometer _sensor;

        private bool _disposed = false;

        /// <summary>
        /// Initializes the sensor
        /// </summary>
        /// <exception cref="NotSupportedException">The device does not support the sensor</exception>
        /// <exception cref="UnauthorizedAccessException">The user does not grant your app access to sensors</exception>
        public ActivityService()
        {
            try
            {
                GetSensorIfPermission();
            }
            catch (NotSupportedException)
            {
                Logger.Error("Pedometer sensor is not supported on this device!", "ActivityService.cs", "ActivityService");
                Application.Current.Exit();
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Error("Pedometer - unauthorized access!", "ActivityService.cs", "ActivityService");
                Application.Current.Exit();
            }
        }

        ~ActivityService()
        {
            Dispose(false);
        }

        private void GetSensorIfPermission()
        {
            PrivacyPermissionStatus response = PrivacyPermissionService.Check(PrivacyPrivilege.HealthInfo);
            if (response == PrivacyPermissionStatus.Granted)
            {
                _sensor = new Pedometer();
                // Add an event handler to the sensor
                _sensor.DataUpdated += OnSensorDataUpdated;
                _sensor.Interval = 1000;
            }
        }

        /// <summary>
        /// Starts the sensor to receive sensor data
        /// </summary>
        public void Start()
        {
            if (_sensor == null)
            {
                GetSensorIfPermission();
            }
            else
            {
                _sensor.Start();
            }
        }

        /// <summary>
        /// Stops receiving sensor data
        /// </summary>
        /// <remarks>
        /// Reduce battery drain by stopping the sensor when it is not needed
        /// </remarks>
        public void Stop()
        {
            if (_sensor != null)
                _sensor.Stop();
            else
                Logger.Error("Pedometer sensor is null!", "ActivityService.cs", "Stop");
        }

        /// <summary>
        /// Releases all resources used by the current instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _sensor.DataUpdated -= OnSensorDataUpdated;
                    _sensor.Dispose();
                }

                _sensor = null;
                _disposed = true;
            }
        }

        /// <summary>
        /// Called when the Step Count has changed
        /// </summary>
        private void OnSensorDataUpdated(object sender, PedometerDataUpdatedEventArgs e)
        {
            Logger.Info($"Step count: {e.StepCount}");
        }
    }
}
