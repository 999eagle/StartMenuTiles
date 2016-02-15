using StartMenuTiles.Mvvm;
using StartMenuTiles.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace StartMenuTiles.ViewModels
{
    class SteamGameListPageViewModel : ViewModelBase
    {
        ObservableCollection<SteamGameListPage_GameTileViewModel> m_gameTiles;
        public ObservableCollection<SteamGameListPage_GameTileViewModel> GameTiles
        {
            get { return m_gameTiles; }
            set { Set(ref m_gameTiles, value); }
        }

        public SteamGameListPageViewModel()
        {
            GameTiles = new ObservableCollection<SteamGameListPage_GameTileViewModel>();

            if (IsInDesignMode)
            {
                var gt = new SteamGameListPage_GameTileViewModel();
                gt.AppId = 377160;
                gt.GameName = "Fallout 4";
                gt.ImageSource = "http://cdn.akamai.steamstatic.com/steam/apps/" + gt.AppId + "/header.jpg";
                GameTiles.Add(gt);
            }
        }

        public override void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter == null) return;
            GameTiles.Clear();
            var games = (JsonArray)TempDataStore.GetInstance().GetObject(Int32.Parse(parameter));
            foreach (var game in games.OrderBy(v => v.GetObject().GetNamedString("name")))
            {
                var g = game.GetObject();
                var gt = new SteamGameListPage_GameTileViewModel();
                gt.AppId = (int)g.GetNamedNumber("appid");
                gt.GameName = g.GetNamedString("name");
                //gt.ImageSource = "http://media.steampowered.com/steamcommunity/public/images/apps/" + gt.AppId + "/" + g.GetNamedString("img_logo_url") + ".jpg";
                gt.ImageSource = "http://cdn.akamai.steamstatic.com/steam/apps/" + gt.AppId + "/header.jpg";
                GameTiles.Add(gt);
            }
        }
    }

    class SteamGameListPage_GameTileViewModel : ViewModelBase
    {
        #region data for the view model
        string m_gameName;
        public string GameName
        {
            get { return m_gameName; }
            set { Set(ref m_gameName, value); }
        }

        string m_imageSource;
        public string ImageSource
        {
            get { return m_imageSource; }
            set { Set(ref m_imageSource, value); }
        }

        int m_appId;
        public int AppId
        {
            get { return m_appId; }
            set { Set(ref m_appId, value); }
        }

        SolidColorBrush m_borderBrush;
        public SolidColorBrush BorderBrush
        {
            get { return m_borderBrush; }
            set { Set(ref m_borderBrush, value); }
        }

        Mvvm.Command m_navCommand;
        public Mvvm.Command NavCommand { get { return m_navCommand ?? (m_navCommand = new Command(ExecuteNav)); } }
        private void ExecuteNav()
        {
            // data is passed as json
            var ja = new JsonObject();
            ja.Add("header", JsonValue.CreateStringValue("Steam"));
            ja.Add("tile_title", JsonValue.CreateStringValue(m_gameName));
            ja.Add("image_wide_url", JsonValue.CreateStringValue(m_imageSource));
            ja.Add("action", JsonValue.CreateStringValue("steam://run/" + m_appId));
            ja.Add("id", JsonValue.CreateStringValue("Steam\\" + m_appId));
            (App.Current as App).NavigationService.Navigate(typeof(CreateTilePage), ja.Stringify());
        }

        Mvvm.Command m_pointerEnteredCommand;
        public Mvvm.Command PointerEnteredCommand { get { return m_pointerEnteredCommand ?? (m_pointerEnteredCommand = new Command(() => { BorderBrush = m_hoverBorderBrush; })); } }

        Mvvm.Command m_pointerExitedCommand;
        public Mvvm.Command PointerExitedCommand { get { return m_pointerExitedCommand ?? (m_pointerExitedCommand = new Command(() => { BorderBrush = m_defaultBorderBrush; })); } }
        #endregion

        static readonly SolidColorBrush m_defaultBorderBrush = new SolidColorBrush(Colors.Transparent);
        static readonly SolidColorBrush m_hoverBorderBrush = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255));

        public SteamGameListPage_GameTileViewModel()
        {
            m_borderBrush = m_defaultBorderBrush;
        }
    }
}
