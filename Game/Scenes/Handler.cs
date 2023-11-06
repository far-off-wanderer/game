namespace Far_Off_Wanderer.Scenes;

using System;
using System.Threading.Tasks;

public abstract class Handler
{
    public Func<Content, Task> Begin { get; protected set; }
    public Action<TimeSpan, Input> Update { get; protected set; }
    public Action<Graphics, bool> Draw { get; protected set; }
    public Action<string> OnNext { get; internal set; }
}

public class Handler<T> : Handler where T : Scene
{
}
