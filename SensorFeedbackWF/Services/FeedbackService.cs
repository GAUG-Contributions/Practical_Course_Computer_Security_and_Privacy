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

        public void UpdateRingFeedback(FeedbackType feedback)
        {
            MessagingCenter.Send(this, "UpdateRingFeedback", feedback);
        }

        /// <summary>
        /// Gives sonic, haptic, or both types of feedback.
        /// </summary>
        /// <param name="feedbackType">
        /// </param>
        public void GiveHapticAndSonicFeedback(FeedbackType feedbackType)
        {
            if (_feedback == null)
                throw new NotSupportedException(Resources.AppResources.ExceptionVibrationServicePredefinedNotSupported);

            // If the pattern is not supported, then NotSupportedException will be thrown.
            // Predefined pattern: "SoftInputPanel" or "WakeUp" and so on. Supported patterns can be found on https://samsung.github.io/TizenFX/stable/api/Tizen.System.Feedback.html.
            _feedback.Play(feedbackType, "SoftInputPanel");
        }
    }
}
