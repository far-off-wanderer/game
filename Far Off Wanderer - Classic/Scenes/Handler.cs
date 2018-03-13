namespace Far_Off_Wanderer.Scenes
{
    using Microsoft.Xna.Framework;
    using System;

    class Handler
    {
        public Action<Scene, GameTime> Update { get; set; }
        public Action<Scene, Graphics> Draw { get; set; }
        public Action<string> OnNext { get; internal set; }
    }

    class Handler<T> : Handler where T : Scene
    {
        public Handler()
        {
            base.Update = (scene, gameTime) => this.Update(scene as T, gameTime);
            base.Draw = (scene, graphics) => this.Draw(scene as T, graphics);
        }
        public new Action<T, GameTime> Update { get; set; }
        public new Action<T, Graphics> Draw { get; set; }
    }
}