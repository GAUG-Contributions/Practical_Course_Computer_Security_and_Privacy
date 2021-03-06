using System;
using System.Collections.Generic;
using SensorFeedback.Services;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;

using Xamarin.Forms;

namespace SensorFeedback.Views
{
    public partial class FeedbackSettingsPage : ContentPage
    {
        public FeedbackSettingsPage()
        {
            InitializeComponent();

            // Initialize sample data and set ItemsSource in ListView.
            // TODO: Change ItemsSource with your own data.
            List<FeedbackSetting> list = new List<FeedbackSetting>();
            list.AddRange(new FeedbackSetting[] {
                new FeedbackSetting("vibration", "Vibration Feedback", FeedbackType.Vibration),
                new FeedbackSetting("sound", "Sound Feedback", FeedbackType.Sound)
            });
            listView.ItemsSource = list;
        }

        // Called when the switch is changed by a click.
        private void OnSwitchChanged(object sender, ToggledEventArgs e)
        {
            // TODO: Insert code to handle the item's switch.

            // Gets the bound item using switch's BindingContext and the switch state.
            // var switchCell = (SwitchCell)sender;
            // Logger.Info($"Item : {switchCell.BindingContext}");
            // Logger.Info($"Switch set : {e.Value});
        }

        // Called once when an item is selected.
        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // TODO: Insert code to handle a list item selected event.
            // Logger.Info($"Selected Color : {e.SelectedItem}");
        }

        // Called every time an item is tapped.
        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            // TODO: Insert code to handle a list item tapped event.
            // Logger.Info($"Tapped Color : {e.Item}");
        }

        private class FeedbackSetting
        {
            private string _name;
            public string DisplayName;
            private FeedbackType _type;
            private bool _isActive;

            public FeedbackSetting(string name, string displayName, FeedbackType type)
            {
                _name = name;
                DisplayName = displayName;
                _type = type;
                _isActive = false;
            }
        }
    }
}
