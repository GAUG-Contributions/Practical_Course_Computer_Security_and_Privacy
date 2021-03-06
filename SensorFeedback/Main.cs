using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace SensorFeedback
{
    class TizenApplication : global::Xamarin.Forms.Platform.Tizen.FormsApplication
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            LoadApplication(new App());
        }

        static void Main(string[] args)
        {
            using (var tizenApplication = new TizenApplication())
            {
                Forms.Init(tizenApplication);
                FormsCircularUI.Init();
                tizenApplication.Run(args);
            }
        }
    }
}
