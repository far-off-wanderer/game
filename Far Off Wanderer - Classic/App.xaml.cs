using Conesoft.Engine.Application;
using Conesoft.Engine.Level;
using Conesoft.Engine.NavigationService;
using Conesoft.Engine.NavigationService.Implementation;
using Conesoft.Engine.ResourceLoader;
using Conesoft.Engine.ResourceLoader.Implementation;
using Microsoft.Xna.Framework.Content;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Far_Off_Wanderer___Classic
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            if (IsXbox && ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Application", "RequiresPointerMode"))
            {
                Application.Current.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
            }

            SetupIoC();
        }

        public static bool IsXbox => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox";

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(GamePage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        public static new App Current { get { return Application.Current as App; } }

        public IApplication Appl { get; private set; }

        public ContentManager Content { get; set; }
        public bool FirstTime { get; internal set; }

        public void SetupIoC()
        {
            var container = TinyIoC.TinyIoCContainer.Current;

            container.AutoRegister();

            container.Register<IResourceLoader>(new ResourceLoader()
            {
                Manager = () => Current.Content
            });

            container.Register<INavigationService<ILevel>>(new NavigationService<ILevel>());

            Appl = container.Resolve<IApplication>();

        }
    }
}
