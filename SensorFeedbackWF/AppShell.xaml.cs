using Xamarin.Forms;

namespace SensorFeedbackWF
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // In case of watch face, the flyout icon is hidden even though there are more than one flyout item.
            // If you want to show it, remove FlyoutBehavior="Disabled" in AppShell.xaml, but it is not recommended.
        }

        protected override bool OnBackButtonPressed()
        {
            if (FlyoutIsPresented)
            {
                FlyoutIsPresented = false;
                return true;
            }
            return base.OnBackButtonPressed();
        }
    }
}
