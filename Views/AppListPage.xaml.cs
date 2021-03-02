using System;
using Tizen.Wearable.CircularUI.Forms;
using WatchOut.Services;
using Xamarin.Forms;
using Tizen.Applications;
using Tizen.Sensor;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace WatchOut.Views
{
    public partial class AppListPage : ContentPage
    {
        public AppListPage()
        {
            InitializeComponent();
            GetAppList();
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
            // Open AppDetail View
            Navigation.PushAsync(new AppDetailPage(((ApplicationInfo)((ListView)sender).SelectedItem).ApplicationId));
        }

        // Called every time an item is tapped.
        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            // TODO: Insert code to handle a list item tapped event.
            // Logger.Info($"Tapped Color : {e.Item}");
        }

        private async void GetAppList()
        {
            IEnumerable <ApplicationInfo> appInfoList = await ApplicationManager.GetInstalledApplicationsAsync();
            List<ApplicationInfo> list = new List<ApplicationInfo>();
            foreach (ApplicationInfo applicationInfo in appInfoList)
            {
                // Filter out apps that aren't displayed on the menu
                if (!applicationInfo.IsNoDisplay)
                    list.Add(applicationInfo);
            }

            listView.ItemsSource = list;
        }
    }
}
