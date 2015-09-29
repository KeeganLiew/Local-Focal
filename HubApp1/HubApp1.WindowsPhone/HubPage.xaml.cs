using HubApp1.Common;
using HubApp1.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Syndication;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace HubApp1
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public HubPage()
        {
            this.InitializeComponent();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroups = await SampleDataSource.GetGroupsAsync();
            this.DefaultViewModel["Groups"] = sampleDataGroups;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Shows the details of a clicked group in the <see cref="SectionPage"/>.
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Details about the click event.</param>
        private void GroupSection_ItemClick(object sender, ItemClickEventArgs e)
        {
            var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            if (!Frame.Navigate(typeof(SectionPage), groupId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        /// <summary>
        /// Shows the details of an item clicked on in the <see cref="ItemPage"/>
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Defaults about the click event.</param>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            if (!Frame.Navigate(typeof(ItemPage), itemId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }
        Geolocator geo = null;

        private async void button1_Click(
            object sender, RoutedEventArgs e)
        {
            if (geo == null)
            {
                geo = new Geolocator();
            }

            Geoposition pos = await geo.GetGeopositionAsync();
            String longi = pos.Coordinate.Point.Position.Latitude.ToString();
            String lat = pos.Coordinate.Point.Position.Longitude.ToString();
            String key = "ArDNO0E1rELgE12UBEwg70t9FP16lT65sZYm_q1n0HH-VkazUdnGMexqcWFXPL9F";

            string town = "Wellington";
            string longAddress = "123 Magic Street";
            string weather = "magicalll";

            String request1 = "http://dev.virtualearth.net/REST/v1/Locations/" + longi + "," + lat + "?o=json&key=" + key;
            String request2 = "http://api.openweathermap.org/data/2.5/weather?q=" + town;
            String request3 = "http://webhose.io/search?token=a8a4f51a-6a3f-4595-adde-c934e53f69f3&q=" + town;//+"&ts=1440944192123"; 

            var uri1 = new Uri(request1);
            var uri2 = new Uri(request2);
            var uri3 = new Uri(request3);
            var httpClient = new HttpClient();
            // Always catch network exceptions for async methods
            try
            {
                textAccuracy.Text = "Refreshing...";
                var result1 = await httpClient.GetStringAsync(uri1);
                int y = result1.IndexOf("postalCode");
                int x = result1.IndexOf("locality\":\"")+11;
                int z = y-x-3;

                int add = result1.IndexOf("formattedAddress\":\"") + 19;
                int local = result1.IndexOf("locality");
                int end = local - add - 3;

                town = result1.Substring(x,z);
                longAddress = result1.Substring(add, end);

                var result2 = await httpClient.GetStringAsync(uri2);
                String desc = "main\":\"";
                int indexDesc = result2.IndexOf(desc) + desc.Length;
                int indexAfterDesc = result2.IndexOf("\"", indexDesc);
                int stringL = indexAfterDesc - indexDesc;

                weather = result2.Substring(indexDesc, stringL);
                //Location.Text = "You are currently at " + longAddress + ". \nThe weather here at " + town + " is " + weather + ".";
                News.Text = "Local News: Loading...";
                var result3 = await httpClient.GetStringAsync(uri3);
                int currIndex = 0;
                int check = result3.IndexOf("\"title\": \"", currIndex);
                String newsTitles = "";

                int storyNum = 1;
                News.Text += ".";
                while (storyNum < 9)
                {
                    check = result3.IndexOf("\"title\": \"", check + 1);
                    check = result3.IndexOf("\"title\": \"", check + 1);
                    check = result3.IndexOf("\"title\": \"", check + 1);
                    check = result3.IndexOf("\"title\": \"", check + 1);
                    News.Text += ".";
                    int start = result3.IndexOf("\"title\": \"", check - 1);
                    int endd = result3.IndexOf("\"title_full", start);
                    String title = result3.Substring(start + 9, endd - start - 19);
                    newsTitles += storyNum + ". " + title + "\n";

                    check = result3.IndexOf("\"title\": \"", check + 1);
                    check = result3.IndexOf("\"title\": \"", check + 1);
                    check = result3.IndexOf("\"title\": \"", check + 1);
                    check = result3.IndexOf("\"title\": \"", check + 1);
                    storyNum++;
                }

                News.Text = "Local News:\n" + newsTitles;
                textAccuracy.Text = "Refreshed successfully";

            }
            catch (Exception ex)
            {
                // Details in ex.Message and ex.HResult. 
                textAccuracy.Text = "Refresh failed. Please refresh again.";
            }

            Location.Text = "You are currently at " + longAddress + ". The weather here at " + town + " is " + weather +".";
            //News.Text += "\nEnd";

        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

       
    }
}