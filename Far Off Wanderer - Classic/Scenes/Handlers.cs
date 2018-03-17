namespace Far_Off_Wanderer.Scenes
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class Handlers
    {
        Dictionary<Type, Handler> SceneHandlers = new Dictionary<Type, Handler>();
        All all;
        Scene current;
        bool active = true;
        bool begin = true;

        public bool IsActive => active;

        public void Update(GameTime gameTime, Graphics graphics)
        {
            if (SceneHandlers.ContainsKey(current.GetType()))
            {
                if (begin)
                {
                    SceneHandlers[current.GetType()].Begin?.Invoke(current, graphics);
                    begin = false;
                }
                SceneHandlers[current.GetType()].Update?.Invoke(current, gameTime);
            }
            else
            {
                active = false;
            }
        }

        public void Draw(Graphics graphics)
        {
            if(SceneHandlers.ContainsKey(current.GetType()) && !begin)
            {
                SceneHandlers[current.GetType()].Draw?.Invoke(current, graphics);
            }
        }

        public void Add<T>(Handler<T> handler) where T : Scene
        {
            Add(typeof(T), handler);
        }

        public void Add(Type type, Handler handler)
        {
            if (SceneHandlers.ContainsKey(type) && SceneHandlers[type] != null)
            {
                var existing = SceneHandlers[type];
                existing.Draw = handler.Draw ?? existing.Draw;
                existing.Update = handler.Update ?? existing.Update;
            }
            else
            {
                SceneHandlers[type] = handler;
            }
            handler.OnNext = RunNext;
        }

        public void RunNext(string scene)
        {
            current = all.Scenes.FirstOrDefault(s => s.Name == scene);
            begin = true;
        }

        public void Run(All all)
        {
            this.all = all;
            this.current = all.Index;
        }
    }
}