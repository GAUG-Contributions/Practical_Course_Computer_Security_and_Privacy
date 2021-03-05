using System;
using System.Runtime.CompilerServices;
using System.Linq;
using Xamarin.Forms;

namespace SensorFeedbackWF.Views
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
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

        }

        // Called every time an item is tapped.
        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            // TODO: Insert code to handle a list item tapped event.
            // Logger.Info($"Tapped Color : {e.Item}");
        }
    }
}
