namespace Far_Off_Wanderer.Scenes
{
    using System;

    public abstract class Handler
    {
        public Action<Content> Begin { get; protected set; }
        public Action<TimeSpan, Input> Update { get; protected set; }
        public Action<Graphics> Draw { get; protected set; }
        public Action<string> OnNext { get; internal set; }
    }

    public class Handler<T> : Handler where T : Scene
    {
    }
}