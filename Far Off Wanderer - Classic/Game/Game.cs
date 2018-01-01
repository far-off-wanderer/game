using Conesoft.Engine.Accelerometer;
using Conesoft.Engine.Level;
using Conesoft.Engine.NavigationService;
using Conesoft.Engine.Resources;
using System;

namespace Conesoft.Game
{
    interface IGame
    {
        INavigationService<ILevel> NavigationService { get; }
        IAccelerometer Accelerometer { get; }
        IResources Resources { get; }
    }

    namespace Implementation
    {
        class Game : IGame
        {
            public INavigationService<ILevel> NavigationService { get; private set; }
            public IAccelerometer Accelerometer { get; private set; }
            public IResources Resources { get; private set; }

            public Game(INavigationService<ILevel> navigationService, IAccelerometer accelerometer, IResources resources)
            {
                NavigationService = navigationService;
                Accelerometer = accelerometer;
                Resources = resources;
            }

            private void Update(TimeSpan timeSpan)
            {
                var current = NavigationService.Current;
                if (current != null)
                {
                    current.Update(timeSpan);
                }
            }

            private void Draw()
            {
                var current = NavigationService.Current;
                if (current != null)
                {
                    current.Draw();
                }
            }
        }
    }
}
