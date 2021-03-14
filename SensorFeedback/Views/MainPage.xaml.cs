using System;
using System.Threading.Tasks;
using SensorFeedback.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SensorFeedback.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage, IDisposable
    { 
        // Used to deallocate resources for the services
        private bool _disposedValue = false;
        private RandomSensingService _randomSensingService;

        public MainPage()
        {
            InitializeComponent();
            _randomSensingService = RandomSensingService.GetInstance;

            // Used to debug - don't remove before the final version
            MessagingCenter.Subscribe<RandomSensingService, String>(this, "printMe", async (sender, str) =>
            {
                await printMe(str);
            });

        }

        // Used to debug - don't remove before the final version
        private Task printMe(string str){
            labelOverview.Text = str;

            return Task.CompletedTask;
        }

        /*================================================================================*/
        /*=============================== BUTTON LISTENERS ===============================*/
        /*================================================================================*/

        protected override bool OnBackButtonPressed()
        {
            //Shell.Current.GoToAsync("//Main");
            return base.OnBackButtonPressed();
        }

        private void OnHealthButtonClicked(object sender, EventArgs args){

        }

        private void OnLocationButtonClicked(object sender, EventArgs args){

        }

        private void OnRandButtonClicked(object sender, EventArgs e)
        {
            if (_randomSensingService.IsRandomActive()){
                buttonRand.TextColor = Color.White;
                _randomSensingService.StopRandom();
            }
            else {
                buttonRand.TextColor = Color.Green;
                _randomSensingService.StartRandom();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _randomSensingService.Dispose();
                }
                _randomSensingService = null;
                _disposedValue = true;
            }
        }

        ~MainPage()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}