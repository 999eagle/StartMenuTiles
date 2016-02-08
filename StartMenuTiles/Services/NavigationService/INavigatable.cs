using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

namespace StartMenuTiles.Services.NavigationService
{
    public interface INavigatable
    {
        void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state);
        Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending);
        void OnNavigatingFrom(NavigatingEventArgs args);
    }
}

