using Tizen.Applications;
using Tizen.Wearable.CircularUI.Forms;
using Tizen.Wearable.CircularUI.Forms.Renderer.Watchface;
using Xamarin.Forms;

namespace SensorFeedbackWF
{
    class TizenWatchFace : FormsWatchface
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            LoadWatchface(new App());
        }

        // Called at least once per second.
        // The TimeTick event can also be used for the same effect.
        protected override void OnTick(TimeEventArgs time)
        {
            base.OnTick(time);
        }

        // Called when ambient mode state is changed.
        // The AmbientChanged event can also be used for the same effect.
        protected override void OnAmbientChanged(AmbientEventArgs mode)
        {
            base.OnAmbientChanged(mode);
        }

        // Called every tick when ambient mode is enabled.
        // You can get the duration of each tick by GetAmbientTickType().
        // The AmbientTick event can also be used for the same effect.
        protected override void OnAmbientTick(TimeEventArgs time)
        {
            base.OnAmbientTick(time);
        }

        static void Main(string[] args)
        {
            using (var tizenWatchFace = new TizenWatchFace())
            {
                Forms.Init(tizenWatchFace);
                FormsCircularUI.Init();
                tizenWatchFace.Run(args);
            }
        }
    }
}
