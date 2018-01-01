using Conesoft.Engine.Level;
using Conesoft.Engine.Random;
using System;

namespace Conesoft.Game.Level
{
    class LastManStandingLevel : ILevel
    {
        IRandom random;

        public LastManStandingLevel(IRandom random)
        {
            this.random = random;
        }

        public void Update(TimeSpan timeSpan)
        {
        }

        public void Draw()
        {
        }
    }
}
