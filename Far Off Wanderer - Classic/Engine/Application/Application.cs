using Conesoft.Engine.Level;
using Conesoft.Engine.NavigationService;

namespace Conesoft.Engine.Application
{
    public interface IApplication
    {
        INavigationService<ILevel> NavigationService { get; }
    }

    namespace Implementation
    {
        class Application : IApplication
        {
            public INavigationService<ILevel> NavigationService { get; private set; }

            public Application(INavigationService<ILevel> navigationService)
            {
                NavigationService = navigationService;
            }
        }
    }
}
