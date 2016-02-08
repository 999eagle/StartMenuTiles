using StartMenuTiles.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace StartMenuTiles.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var viewModel = new MainPageViewModel();
            viewModel.TileSources.Add(new MainPage_TileSourceViewModel { ImageSource = "ms-appx:///Assets/Steam.png", Header = "Steam", Description = "Pin Steam games", PageType = typeof(SteamTilePage) });
            viewModel.TileSources.Add(new MainPage_TileSourceViewModel { ImageSource = "ms-appx:///Assets/Origin.png", Header = "Origin", Description = "Pin Origin games" });
            DataContext = viewModel;
        }
    }
}
