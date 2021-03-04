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
        public void ShowRingFeedback()
        {
            MessagingCenter.Send(this, "ShowRingFeedback");
        }

        public void StopRingFeedback()
        {
            MessagingCenter.Send(this, "StopRingFeedback");
        }
    }
}
