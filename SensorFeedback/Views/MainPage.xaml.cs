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
        }

        /*================================================================================*/
        /*=============================== BUTTON LISTENERS ===============================*/
        /*================================================================================*/

        protected override bool OnBackButtonPressed()
        {
            //Shell.Current.GoToAsync("//Main");
            return base.OnBackButtonPressed();
        }

        private void OnRandButtonClicked(object sender, EventArgs e)
        {
            // Process the button click 
            if (_randomSensingService != null){
                if (_randomSensingService.IsRandomActive())
                {
                    buttonRand.TextColor = Color.White;
                    _randomSensingService.StopRandom();
                }
                else
                {
                    buttonRand.TextColor = Color.Green;
                    _randomSensingService.StartRandom();
                }
            } 
            else{
                // If for some reason the random sensing service was not 
                // successfully initialized inside the MainPage(), try once more
                _randomSensingService = RandomSensingService.GetInstance;
                // If the initialization is successful now, process the pressed button
                if(_randomSensingService != null)
                    OnRandButtonClicked(sender, e);
                else
                {
                    // The initialization failed again
                    Logger.Error("Random sensing service cannot be initialized!", "MainPage.xaml.cs", "OnRandButtonClicked");
                    // Application.Current.Quit(); - the application could be closed at this point, because the button is not working anyway
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_randomSensingService != null)
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