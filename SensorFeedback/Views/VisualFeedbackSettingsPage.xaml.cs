using SensorFeedback.Models;
using SensorFeedback.Services;
using System;
using System.Collections.Generic;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static SensorFeedback.Services.RandomSensingService;

namespace SensorFeedback.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VisualFeedbackSettingsPage : ContentPage
    {
        private DatabaseService _ds;
        private UserSettings _userSettings;
        private List<VisualFeedbackSetting> _visualFeedbackSettings = new List<VisualFeedbackSetting>();
        List<VisualFeedbackSetting> VisualFeedbackSettings { get { return _visualFeedbackSettings; } }

        public VisualFeedbackSettingsPage()
        {
            InitializeComponent();

            _ds = DatabaseService.GetInstance;
            LoadSettingsFromDB();

            // Populate the list view with different feedback options
            // The Radio button value for each entry is read from the db.
            _visualFeedbackSettings.Add(new VisualFeedbackSetting
            {
                DisplayName = "Ring",
                Type = RandomSensingService.VisualFeedback.Ring,
                IsChecked = _userSettings.VisualFeedbackType == RandomSensingService.VisualFeedback.Ring
            });
            _visualFeedbackSettings.Add(new VisualFeedbackSetting
            {
                DisplayName = "Icon",
                Type = RandomSensingService.VisualFeedback.Icon,
                IsChecked = _userSettings.VisualFeedbackType == RandomSensingService.VisualFeedback.Icon
            });
            _visualFeedbackSettings.Add(new VisualFeedbackSetting
            {
                DisplayName = "Notification",
                Type = RandomSensingService.VisualFeedback.Notification,
                IsChecked = _userSettings.VisualFeedbackType == RandomSensingService.VisualFeedback.Notification
            });

            listView.ItemsSource = _visualFeedbackSettings;
        }

        // Called when the switch is changed by a click.
        private void OnRadioButtonPressed(object sender, EventArgs e)
        {
             _userSettings.VisualFeedbackType = ((VisualFeedbackSetting)((RadioCell)sender).BindingContext).Type;
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

        private class VisualFeedbackSetting
        {
            public string DisplayName { get; set; }
            public VisualFeedback Type { get; set; }
            public bool IsChecked { get; set; }
        }
    }
}