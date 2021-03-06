using SensorFeedback.Services;
using Xamarin.Forms;

namespace SensorFeedback
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // By default, Shell displays a flyout icon when it contains more than one item.
            // If you want to hide it, set FlyoutBehavior.Disabled as shown below.
            // FlyoutBehavior = FlyoutBehavior.Disabled
        }

        protected override bool OnBackButtonPressed()
        {
            if (FlyoutIsPresented)
            {
                FlyoutIsPresented = false;
                return true;
            }
            RandomSensingService rss = RandomSensingService.GetInstance;
            rss.AllowSensing(false);
            return base.OnBackButtonPressed();
        }
    }
}
