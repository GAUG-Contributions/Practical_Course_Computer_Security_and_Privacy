using SensorFeedbackWF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace SensorFeedbackWF.Services
{
    public class FeedbackService : IDisposable
    {
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

        public void ReceiveRingFeedback(string visualFeedback, string vibrationFeedback, string soundFeedback)
        {
            // Forward the visual feedback to the Main view
            MessagingCenter.Send(this, "ReceiveRingFeedback", visualFeedback);
            // Activate Haptic and/or sonic feedback
            bool bVibration = false, bSound = false;
            _ = bool.TryParse(vibrationFeedback, out bVibration);
            _ = bool.TryParse(soundFeedback, out bSound);

            if(visualFeedback != "NoFeedback")
                GiveHapticAndSonicFeedback(bVibration, bSound);
        }

        /// <summary>
        /// Gives sonic, haptic, or both types of feedback.
        /// </summary>
        /// <param name="feedbackType">
        /// </param>
        public void GiveHapticAndSonicFeedback(bool vibration, bool sound)
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
            if(vibration || sound)
                _feedback.Play(ft, "SoftInputPanel");
        }
    }
}
