using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Far_Off_Wanderer
{
    public sealed partial class GamePage : Page
    {
        public GamePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var game = MonoGame.Framework.XamlGame<Startup>.Create(e.Parameter as string, Window.Current.CoreWindow, swapChainPanel);

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (_, __) => game.GoBack();

            base.OnNavigatedTo(e);
        }
    }
}
