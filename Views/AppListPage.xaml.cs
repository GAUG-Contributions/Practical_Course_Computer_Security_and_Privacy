using System;
using Tizen.Wearable.CircularUI.Forms;
using WatchOut.Services;
using Xamarin.Forms;
using Tizen.Applications;
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

            // Initialize sample data and set ItemsSource in ListView.
            // TODO: Change ItemsSource with your own data.
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
            ApplicationInfoFilter appInfoFilterWebapp = new ApplicationInfoFilter();
            appInfoFilterWebapp.Filter.Add(ApplicationInfoFilter.Keys.Type, "webapp");
            IEnumerable<ApplicationInfo> appInfoListWebApp = await ApplicationManager.GetInstalledApplicationsAsync(appInfoFilterWebapp);

            ApplicationInfoFilter appInfoFilterDotnet = new ApplicationInfoFilter();
            appInfoFilterDotnet.Filter.Add(ApplicationInfoFilter.Keys.Type, "dotnet");
            IEnumerable<ApplicationInfo> appInfoListDotnet = await ApplicationManager.GetInstalledApplicationsAsync();

            listView.ItemsSource = appInfoListWebApp.Union(appInfoListDotnet);
        }
    }
}
