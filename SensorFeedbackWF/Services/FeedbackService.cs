using SensorFeedbackWF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace SensorFeedbackWF.Services
{
    public class FeedbackService
    {
        public void ShowRingFeedback(MainPage.FeedbackType feedback)
        {
            MessagingCenter.Send(this, "ShowRingFeedback", feedback);
        }

        private void StopRingFeedback()
        {
            MessagingCenter.Send(this, "StopRingFeedback");
        }
    }
}
