using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartMenuTiles.Mvvm;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace StartMenuTiles.ViewModels
{
    class MainPageViewModel : ViewModelBase
    {
        ObservableCollection<MainPage_TileSourceViewModel> m_tileSources;
        public ObservableCollection<MainPage_TileSourceViewModel> TileSources
        {
            get { return m_tileSources; }
            set { Set(ref m_tileSources, value); }
        }

        public MainPageViewModel()
        {
            TileSources = new ObservableCollection<MainPage_TileSourceViewModel>();

            if (IsInDesignMode)
            {
                // test data for design time
                TileSources.Add(new MainPage_TileSourceViewModel { ImageSource = "ms-appx:///Assets/Steam.png", Header = "Steam", Description = "Pin Steam games" });
                TileSources.Add(new MainPage_TileSourceViewModel { ImageSource = "ms-appx:///Assets/Origin.png", Header = "Origin", Description = "Pin Origin games" });
            }
        }
    }

    class MainPage_TileSourceViewModel : ViewModelBase
    {
        #region data for the view model
        string m_header;
        public string Header
        {
            get { return m_header; }
            set { Set(ref m_header, value); }
        }

        string m_description;
        public string Description
        {
            get { return m_description; }
            set { Set(ref m_description, value); }
        }

        string m_imageSource;
        public string ImageSource
        {
            get { return m_imageSource; }
            set { Set(ref m_imageSource, value); }
        }

        Type m_pageType;
        public Type PageType
        {
            get { return m_pageType; }
            set { Set(ref m_pageType, value); }
        }

        SolidColorBrush m_borderBrush;
        public SolidColorBrush BorderBrush
        {
            get { return m_borderBrush; }
            set { Set(ref m_borderBrush, value); }
        }

        Mvvm.Command m_navCommand;
        public Mvvm.Command NavCommand { get { return m_navCommand ?? (m_navCommand = new Command(ExecuteNav, () => m_pageType != null)); } }
        private void ExecuteNav()
        {
            (App.Current as App).NavigationService.Navigate(m_pageType);
        }

        Mvvm.Command m_pointerEnteredCommand;
        public Mvvm.Command PointerEnteredCommand { get { return m_pointerEnteredCommand ?? (m_pointerEnteredCommand = new Command(() => { BorderBrush = m_hoverBorderBrush; })); } }

        Mvvm.Command m_pointerExitedCommand;
        public Mvvm.Command PointerExitedCommand { get { return m_pointerExitedCommand ?? (m_pointerExitedCommand = new Command(() => { BorderBrush = m_defaultBorderBrush; })); } }
        #endregion

        static readonly SolidColorBrush m_defaultBorderBrush = new SolidColorBrush(Colors.Transparent);
        static readonly SolidColorBrush m_hoverBorderBrush = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255));

        public MainPage_TileSourceViewModel()
        {
            m_borderBrush = m_defaultBorderBrush;
        }
    }
}
