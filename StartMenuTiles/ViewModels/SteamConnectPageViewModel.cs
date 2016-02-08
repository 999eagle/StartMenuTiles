using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using StartMenuTiles.Mvvm;
using StartMenuTiles.Views;

namespace StartMenuTiles.ViewModels
{
    class SteamConnectPageViewModel : ViewModelBase
    {
        string m_profileUri;
        public string ProfileUri
        {
            get { return m_profileUri; }
            set { Set(ref m_profileUri, value); }
        }

        string m_errorMessage;
        public string ErrorMessage
        {
            get { return m_errorMessage; }
            set { Set(ref m_errorMessage, value); }
        }

        string m_errorHint;
        public string ErrorHint
        {
            get { return m_errorHint; }
            set { Set(ref m_errorHint, value); }
        }

        Visibility m_buttonVisibility = Visibility.Visible;
        public Visibility ButtonVisibility
        {
            get { return m_buttonVisibility; }
            set { Set(ref m_buttonVisibility, value); }
        }

        Visibility m_spinnerVisibility = Visibility.Collapsed;
        public Visibility SpinnerVisibility
        {
            get { return m_spinnerVisibility; }
            set { Set(ref m_spinnerVisibility, value); }
        }

        Mvvm.Command m_connectCommand;
        public Mvvm.Command ConnectCommand { get { return m_connectCommand ?? (m_connectCommand = new Command(ExecuteConnect)); } }
        private async void ExecuteConnect()
        {
            ButtonVisibility = Visibility.Collapsed;
            SpinnerVisibility = Visibility.Visible;
            ErrorMessage = "";
            ErrorHint = "";

            var steamClient = await SteamWebApi.GetInstance();
            var steamid = await steamClient.ISteamUser_ResolveVanityUrl(m_profileUri);
            if (!steamid.Success)
            {
                ButtonVisibility = Visibility.Visible;
                SpinnerVisibility = Visibility.Collapsed;
                ErrorMessage = "Invalid profile URL";
                ErrorHint = "Did you make a typo?";
                return;
            }
            var games = await steamClient.IPlayerService_GetOwnedGames(steamid.Result);
            if (!games.Success)
            {
                ButtonVisibility = Visibility.Visible;
                SpinnerVisibility = Visibility.Collapsed;
                ErrorMessage = "Couldn't fetch game list";
                ErrorHint = "Is your profile public?";
                return;
            }

            var nav = (App.Current as App).NavigationService;
            int key = TempDataStore.GetInstance().StoreObject(games.Result);
            nav.Navigate(typeof(SteamGameListPage), key.ToString());
        }
    }
}
