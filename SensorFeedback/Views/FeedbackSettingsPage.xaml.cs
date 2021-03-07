using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SensorFeedback.Models;
using SensorFeedback.Services;
using SQLite;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;

using Xamarin.Forms;

namespace SensorFeedback.Views
{
    public partial class FeedbackSettingsPage : ContentPage
    {
        private DatabaseService _ds;
        private UserSettings _userSettings;
        private ObservableCollection<FeedbackSetting> _feedbackSettings = new ObservableCollection<FeedbackSetting>();
        ObservableCollection<FeedbackSetting> FeedbackSettings { get { return _feedbackSettings; } }

        public FeedbackSettingsPage()
        {
            InitializeComponent();
            listView.ItemsSource = _feedbackSettings;

            _ds = DatabaseService.GetInstance;
            LoadSettingsFromDB();

            // Populate the list view with different feedback options
            // The switch value for each entry is read from the db.
            _feedbackSettings.Add(new FeedbackSetting { 
                DisplayName = "Vibration Feedback", 
                Name = "vibration", 
                IsActive = _userSettings.ActivateVibrationFeedback
            });
            _feedbackSettings.Add(new FeedbackSetting { 
                DisplayName = "Sound Feedback", 
                Name = "sound", 
                IsActive = _userSettings.ActivateSoundFeedback
            });
        }

        // Called when the switch is changed by a click.
        private void OnSwitchChanged(object sender, ToggledEventArgs e)
        {
            if (string.Equals(((FeedbackSetting)((SwitchCell)sender).BindingContext).Name, "vibration", StringComparison.OrdinalIgnoreCase))
                _userSettings.ActivateVibrationFeedback = e.Value;
            else if (string.Equals(((FeedbackSetting)((SwitchCell)sender).BindingContext).Name, "sound", StringComparison.OrdinalIgnoreCase))
                _userSettings.ActivateSoundFeedback = e.Value;
            UpdateDB();
        }

        private void LoadSettingsFromDB()
        {
            _userSettings = _ds.LoadUserSettings();
        }

        private void UpdateDB()
        {
            try
            {
                if (!_userSettings.Equals(null))
                    _ds.UpdateUserSettings(_userSettings);
                else
                    _ds.InsertUserSettings(_userSettings);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
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
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public FeedbackType Type { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
