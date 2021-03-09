using SensorFeedbackWF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Tizen.Applications;

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
            parseStrings(cStr, ref color, vStr, ref visual, vibStr, ref vibration, sStr, ref sound);
            processFeedbackRequest(color, visual, vibration, sound);
        }

        // Parse the received strings to their appropriate types
        private void parseStrings(string cStr, ref ColorFeedback c, string vStr, ref VisualFeedback v, string vibStr, ref bool vib, string sStr, ref bool s) {

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

        // Process the feedback reques accordingly
        public void processFeedbackRequest(ColorFeedback c, VisualFeedback v, bool vibration, bool sound)
        {
            processVisual(v);
            if(v == VisualFeedback.Ring)
                processColorRing(c);
            
            if(c != ColorFeedback.NoFeedback && v != VisualFeedback.NoFeedback)
                processVibrationAndSound(vibration, sound);

        }

        private void processColorRing(ColorFeedback c){
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
            if (cI == 0){
                // this case is handled by default values
            }

            // If a single sensor is active
            if(cI > 0 && cI < 4){
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

                if (c == ColorFeedback.HealthAndLocation){
                    barColor = healthColor;
                    backgroundColor = locationColor;
                }else if (c == ColorFeedback.HealthAndActivity){
                    barColor = healthColor;
                    backgroundColor = activityColor;
                }else if (c == ColorFeedback.LocationAndActivity){
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

        // TODO: Set appropriate settings for Icon
        private void processColorIcon(ColorFeedback c){

            // TODO: New method should be implemented in the MainPage.cs to accept these settings
            // MessagingCenter.Send(this, "ReceiveIconSettings", message);
        }

        // TODO: Set appropriate settings for Notification
        private void processColorNotification(ColorFeedback c){

            // TODO: New method should be implemented in the MainPage.cs to accept these settings
            // MessagingCenter.Send(this, "ReceiveNotificationSettings", message);
        }

        // TODO: Set appropriate settings for Visual Feedback - this method might not be needed
        private void processVisual(VisualFeedback v){

        }

        private void processVibrationAndSound(bool vibration, bool sound){
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
