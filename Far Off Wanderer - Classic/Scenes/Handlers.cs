namespace Far_Off_Wanderer.Scenes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class SceneHandlers
    {
        Dictionary<Type, Type> handlers;
        Handler current;
        All all;
        Content content;
        Action onExit;

        public void Update(TimeSpan timeSpan, Input input) => current.Update?.Invoke(timeSpan, input);
        public void Draw(Graphics graphics) => current.Draw?.Invoke(graphics);

        public Handler Create(Scene scene)
        {
            var handlerType = handlers[scene.GetType()];
            var handler = Activator.CreateInstance(handlerType, scene) as Handler;
            handler.Begin?.Invoke(content);
            handler.OnNext = RunNext;
            return handler;
        }

        public void RunNext(string sceneName)
        {
            var scene = all.Scenes.FirstOrDefault(s => s.Name == sceneName);
            if (scene != null)
            {
                current = Create(scene);
            }
            else
            {
                onExit();
            }
        }

        public SceneHandlers(Action onExit)
        {
            this.onExit = onExit;

            var handlerTypes = typeof(Handler).GetTypeInfo().Assembly.DefinedTypes
                       .Where(t => t.IsSubclassOf(typeof(Handler)) && t.IsGenericType == false && t.BaseType.GetTypeInfo().IsGenericType);

            handlers = handlerTypes.Select(handler => new
            {
                SceneType = handler.BaseType.GetTypeInfo().GenericTypeArguments.First(),
                HandlerType = handler.AsType()
            }).ToDictionary(item => item.SceneType, item => item.HandlerType);
        }

        public void Run(All all, Content content)
        {
            this.all = all;
            this.content = content;
            this.current = Create(all.Index);
        }
    }
}