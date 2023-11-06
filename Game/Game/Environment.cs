namespace Far_Off_Wanderer.Game;

using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Drawing;

public class Environment
{
    public Size ScreenSize { get; set; }
    public bool Flipped { get; set; }
    public Random Random { get; set; } = new();
    public Dictionary<string, SoundEffect> Sounds { get; set; }
    public Camera ActiveCamera { get; set; }
    public float Range { get; set; }
    public LevelHandler.InputActions Actions { get; set; }
}
