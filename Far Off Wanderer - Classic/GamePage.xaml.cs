using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Far_Off_Wanderer
{
    public sealed partial class GamePage : Page
    {
        private Startup game;

        public GamePage()
        {
            this.InitializeComponent();

            game = MonoGame.Framework.XamlGame<Startup>.Create(string.Empty, Window.Current.CoreWindow, swapChainPanel);
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += GamePage_BackRequested;
        }

        private void GamePage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            game.GoBack();
        }
    }
}
