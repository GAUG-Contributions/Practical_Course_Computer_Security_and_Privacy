using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SensorFeedbackWF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FeedbackSettings : ContentPage
    {
        public FeedbackSettings()
        {
            InitializeComponent();
        }

        private void OnSwitchChanged(object sender, CheckedChangedEventArgs e)
        {

        }
    }
}