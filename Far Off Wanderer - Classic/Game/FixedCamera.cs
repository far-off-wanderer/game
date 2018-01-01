using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Conesoft.Game
{
    public class FixedCamera : Camera
    {
        public override Microsoft.Xna.Framework.Vector3 Target { get; set; }
    }
}
