using System;
using Tizen.Applications;
using Tizen.Sensor;

namespace SensorFeedback.Services
{
    // For more information about sensors see https://docs.tizen.org/application/dotnet/guides/location-sensors/device-sensors
    public class HeartRateMonitorService : IDisposable
    {
        private HeartRateMonitor _sensor;

        private bool _disposed = false;

        /// <summary>
        /// Initializes the sensor
        /// </summary>
        /// <exception cref="NotSupportedException">The device does not support the sensor</exception>
        /// <exception cref="UnauthorizedAccessException">The user does not grant your app access to sensors</exception>
        public HeartRateMonitorService()
        {
            try
            {
                GetSensorIfPermission();
            }
            catch (NotSupportedException)
            {
                Application.Current.Exit();
            }
            catch (UnauthorizedAccessException)
            {
                Application.Current.Exit();
            }
        }

        private void GetSensorIfPermission()
        {
            PrivacyPermissionStatus response = PrivacyPermissionService.Check(PrivacyPrivilege.HealthInfo);
            if (response == PrivacyPermissionStatus.Granted)
            {
                _sensor = new HeartRateMonitor();
                // Add an event handler to the sensor
                _sensor.DataUpdated += OnSensorDataUpdated;
                _sensor.Interval = 1000;
            }
        }

        ~HeartRateMonitorService()
        {
            Dispose(false);
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
            _sensor.Stop();
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
        /// Called when the heart rate has changed
        /// </summary>
        private void OnSensorDataUpdated(object sender, HeartRateMonitorDataUpdatedEventArgs e)
        {
            
            // More details at https://docs.tizen.org/application/dotnet/guides/location-sensors/device-sensors#heart-rate-monitor-sensor
            Logger.Info($"Heart rate: {e.HeartRate}");
        }
    }
}
