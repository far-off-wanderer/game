namespace Far_Off_Wanderer.Game;

using System;

public abstract class Player
{
    public Object3D ControlledObject { get; set; }
    public abstract void UpdateThinking(TimeSpan timeSpan, Environment playerEnvironment);
}
