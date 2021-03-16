using System;
using System.Text.RegularExpressions;
using Tizen.Applications;
using Tizen.Applications.Notifications;
using Tizen.System;
using Xamarin.Forms;

namespace SensorFeedbackWF.Services
{
    public class FeedbackService : IDisposable
    {
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

        private Color healthColor = Color.Red; // #ff0000
        private Color locationColor = Color.Yellow; // #ffff00
        private Color activityColor = Color.Blue; // #0000ff
        private Color noFeedbackColor = Color.Black; // #000000

        private Feedback _feedback = null;

        public FeedbackService()
        {
            try
            {
                _feedback = new Feedback();
            }
            catch (NotSupportedException)
            {
                // This exception is not handled here,
                // instead NotSupportedException will be thrown by GiveHapticAndSonicFeedback().
            }
        }

        ~FeedbackService()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by the current instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance.
        /// </summary>
        /// <param name="disposing">Indicates whether the method call comes from a Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _feedback = null;
            }
        }

        // Receives a feedback request and parses the request in appropriate form
        // This function is called inside OnMessageReceived() inside MessagePortService.cs 
        public void ReceiveFeedbackRequest(string cStr, string vStr, string vibStr, string sStr)
        {
            ColorFeedback color = ColorFeedback.Error;
            VisualFeedback visual = VisualFeedback.Error;
            bool vibration = false;
            bool sound = false;

            // Parses and assigns appropriate values to the feedback variables
            ParseStrings(cStr, ref color, vStr, ref visual, vibStr, ref vibration, sStr, ref sound);
            processFeedbackRequest(color, visual, vibration, sound);
        }

        // Parse the received strings to their appropriate types
        private void ParseStrings(string cStr, ref ColorFeedback c, string vStr, ref VisualFeedback v, string vibStr, ref bool vib, string sStr, ref bool s)
        {
            // Parse color string, if failed assign Error
            if (!Enum.TryParse(cStr, out c))
                c = ColorFeedback.Error;

            // Parse visual string, if failed assign Error
            if (!Enum.TryParse(vStr, out v))
                v = VisualFeedback.Error;

            // Parse vibration string, if failed assign False
            if (!bool.TryParse(vibStr, out vib))
                vib = false;

            // Parse sound string, if failed assign False
            if (!bool.TryParse(sStr, out s))
                s = false;
        }

        // Process the feedback request accordingly
        public void processFeedbackRequest(ColorFeedback c, VisualFeedback v, bool vibration, bool sound)
        {
            if (v == VisualFeedback.Ring)
                ProcessColorRing(c);
            if (v == VisualFeedback.Icon)
                processColorIcon(c);
            if (v == VisualFeedback.Notification)
                ProcessColorNotification(c);

            // Notifications have their own vibration and sound
            if (c != ColorFeedback.NoFeedback && v != VisualFeedback.NoFeedback && v != VisualFeedback.Notification)
                ProcessVibrationAndSound(vibration, sound);
        }

        private void ProcessColorRing(ColorFeedback c)
        {
            // Default values
            bool isVisible = false;
            double barValue = 0.0;
            Color barColor = noFeedbackColor;
            Color backgroundColor = noFeedbackColor;

            // Cast the ColorFeedback to an integer
            // to easily handle the main cases
            int cI = (int)c;

            // If the color feeback is not in the correct format - return
            if (cI >= 7) return;

            // No sensor is active
            if (cI == 0)
            {
                // this case is handled by default values
            }

            // If a single sensor is active
            if (cI > 0 && cI < 4)
            {
                barValue = 1.0;

                if (c == ColorFeedback.Health) barColor = healthColor;
                if (c == ColorFeedback.Location) barColor = locationColor;
                if (c == ColorFeedback.Activity) barColor = activityColor;

                backgroundColor = Color.Transparent;
                isVisible = true;
            }

            // If two sensors are active
            if (cI > 3 && cI < 7)
            {
                barValue = 0.5;

                if (c == ColorFeedback.HealthAndLocation)
                {
                    barColor = healthColor;
                    backgroundColor = locationColor;
                }
                else if (c == ColorFeedback.HealthAndActivity)
                {
                    barColor = healthColor;
                    backgroundColor = activityColor;
                }
                else if (c == ColorFeedback.LocationAndActivity)
                {
                    barColor = locationColor;
                    backgroundColor = activityColor;
                }

                isVisible = true;
            }

            // Create a new message bundle
            Bundle message = new Bundle();
            message.AddItem("isVisible", isVisible.ToString());
            message.AddItem("barValue", barValue.ToString());
            message.AddItem("barColor", barColor.ToHex());
            message.AddItem("backgroundColor", backgroundColor.ToHex());

            // Forward the ring settings message to the Main Page
            MessagingCenter.Send(this, "ReceiveRingSettings", message);
            message.Dispose();
        }

        private void processColorIcon(ColorFeedback c)
        {
            bool bHealthIconActive = false;
            bool bLocationIconActive = false;
            bool bActivityIconActive = false;

            switch (c)
            {
                case ColorFeedback.Health: bHealthIconActive = true; break;
                case ColorFeedback.Location: bLocationIconActive = true; break;
                case ColorFeedback.Activity: bActivityIconActive = true; break;
                case ColorFeedback.HealthAndActivity:
                    bHealthIconActive = true;
                    bActivityIconActive = true;
                    break;
                case ColorFeedback.HealthAndLocation:
                    bHealthIconActive = true;
                    bLocationIconActive = true;
                    break;
                case ColorFeedback.LocationAndActivity:
                    bLocationIconActive = true;
                    bActivityIconActive = true;
                    break;
            }

            // Create a new message bundle
            Bundle message = new Bundle();
            message.AddItem("bHealthIconActive", bHealthIconActive.ToString());
            message.AddItem("bLocationIconActive", bLocationIconActive.ToString());
            message.AddItem("bActivityIconActive", bActivityIconActive.ToString());
            MessagingCenter.Send(this, "ReceiveIconSettings", message);
            message.Dispose();

        }

        private void ProcessColorNotification(ColorFeedback c)
        {
            // If no feedback, don't push a notification
            if (c.Equals(ColorFeedback.NoFeedback)) return;

            // Here, no need to forward messages to the main page as we have nothing to visualize - notifications are treated internally by the system
            // Split the enum value in words
            string[] sensors = Regex.Split(c.ToString(), @"(?<!^)(?=[A-Z])");
            string sWords = string.Join(" ", sensors).ToLower();

            // When multiple sensors are used at once, the icon path will not be found. This is ok, because we can't display more than 1 icon at the same time.
            // As a result, no icon is displayed when 2 sensors are used at the same time.
            Notification notification = new Notification
            {
                Title = (sensors.Length > 1 ? "Sensors" : "Sensor") + " active",
                Content = "Your " + sWords + " data " + (sensors.Length > 1 ? "are" : "is") + " being collected",
                Icon = Tizen.Applications.Application.Current.DirectoryInfo.SharedResource + sWords + ".png",
                Count = 3
            };

            Notification.AccessorySet accessory = new Notification.AccessorySet
            {
                SoundOption = AccessoryOption.On,
                CanVibrate = true
            };

            notification.Accessory = accessory;
            NotificationManager.Post(notification);
            notification.Dispose();
        }

        private void ProcessVibrationAndSound(bool vibration, bool sound)
        {
            if (_feedback == null)
                throw new NotSupportedException(Resources.AppResources.ExceptionVibrationServicePredefinedNotSupported);

            FeedbackType ft = FeedbackType.All;
            if (vibration && sound)
                ft = FeedbackType.All;
            else if (vibration)
                ft = FeedbackType.Vibration;
            else if (sound)
                ft = FeedbackType.Sound;

            // If the pattern is not supported, then NotSupportedException will be thrown.
            // Predefined pattern: "SoftInputPanel" or "WakeUp" and so on. Supported patterns can be found on https://samsung.github.io/TizenFX/stable/api/Tizen.System.Feedback.html.
            if (vibration || sound)
                _feedback.Play(ft, "SoftInputPanel");
        }

    }
}
