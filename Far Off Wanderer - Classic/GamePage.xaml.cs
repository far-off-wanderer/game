using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Far_Off_Wanderer
{
    public sealed partial class GamePage : Page
    {
        public GamePage()
        {
            this.InitializeComponent();

            var game = MonoGame.Framework.XamlGame<Startup>.Create(string.Empty, Window.Current.CoreWindow, swapChainPanel);

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (sender, e) => game.GoBack();
        }
    }
}
