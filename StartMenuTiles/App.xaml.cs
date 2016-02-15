using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Json;
using Windows.System;

namespace StartMenuTiles
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Common.BootStrapper
    {
        public App() : base()
        {
            this.InitializeComponent();
        }

        public override Task OnInitializeAsync()
        {
            Window.Current.Content = new Views.Shell(this.RootFrame);
            return base.OnInitializeAsync();
        }

        public override Task OnStartAsync(StartKind startKind, IActivatedEventArgs e)
        {
            this.NavigationService.Navigate(typeof(Views.MainPage));
            return Task.FromResult<object>(null);
        }

        public override async Task<bool> OnLaunchAsync(ILaunchActivatedEventArgs e)
        {
            if (e.TileId.StartsWith("Run_"))
            {
                JsonObject args = JsonObject.Parse(e.Arguments);
                if (args.ContainsKey("url"))
                {
                    var url = args.GetNamedString("url");
                    // LaunchUriAsynch returns false if launch failed
                    return !(await Launcher.LaunchUriAsync(new Uri(url)));
                }
            }
            return true;
        }
    }
}
