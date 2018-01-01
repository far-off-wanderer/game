using System;
using System.Collections.Generic;

namespace Conesoft.Game
{
    public abstract class Player
    {
        public Object3D ControlledObject { get; set; }
        public abstract void UpdateThinking(TimeSpan timeSpan, DefaultEnvironment playerEnvironment);
    }
}
