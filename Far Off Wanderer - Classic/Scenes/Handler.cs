namespace Far_Off_Wanderer.Scenes
{
    using System;

    class Handler
    {
        public Action<Scene, Content> Begin { get; set; }
        public Action<Scene, TimeSpan> Update { get; set; }
        public Action<Scene, Graphics> Draw { get; set; }
        public Action<string> OnNext { get; internal set; }
    }

    class Handler<T> : Handler where T : Scene
    {
        public Handler()
        {
            base.Begin = (scene, graphics) => this.Begin?.Invoke(scene as T, graphics);
            base.Update = (scene, gameTime) => this.Update?.Invoke(scene as T, gameTime);
            base.Draw = (scene, graphics) => this.Draw?.Invoke(scene as T, graphics);
        }
        public new Action<T, Content> Begin { get; set; }
        public new Action<T, TimeSpan> Update { get; set; }
        public new Action<T, Graphics> Draw { get; set; }
    }
}