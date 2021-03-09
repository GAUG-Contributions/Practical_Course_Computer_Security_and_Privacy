using System;
using System.Collections.Generic;
using System.Windows.Input;
using SensorFeedback.Models;
using SensorFeedback.Services;
using Tizen.System;
using Xamarin.Forms;

namespace SensorFeedback.Views
{
    public partial class OtherFeedbackSettingsPage : ContentPage
    {
        private DatabaseService _ds;
        private UserSettings _userSettings;
        private List<OtherFeedbackSetting> _feedbackSettings = new List<OtherFeedbackSetting>();
        List<OtherFeedbackSetting> FeedbackSettings { get { return _feedbackSettings; } }

        public OtherFeedbackSettingsPage()
        {
            InitializeComponent();

            _ds = DatabaseService.GetInstance;
            LoadSettingsFromDB();

            // Populate the list view with different feedback options
            // The switch value for each entry is read from the db.
            _feedbackSettings.Add(new OtherFeedbackSetting { 
                DisplayName = "Vibration Feedback", 
                Name = "vibration", 
                IsActive = _userSettings.ActivateVibrationFeedback
            });
            _feedbackSettings.Add(new OtherFeedbackSetting { 
                DisplayName = "Sound Feedback", 
                Name = "sound", 
                IsActive = _userSettings.ActivateSoundFeedback
            });

            listView.ItemsSource = _feedbackSettings;
        }

        // Called when the switch is changed by a click.
        private void OnSwitchChanged(object sender, ToggledEventArgs e)
        {
            if (string.Equals(((OtherFeedbackSetting)((SwitchCell)sender).BindingContext).Name, "vibration", StringComparison.OrdinalIgnoreCase))
                _userSettings.ActivateVibrationFeedback = e.Value;
            else if (string.Equals(((OtherFeedbackSetting)((SwitchCell)sender).BindingContext).Name, "sound", StringComparison.OrdinalIgnoreCase))
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

        private class OtherFeedbackSetting
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public FeedbackType Type { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
