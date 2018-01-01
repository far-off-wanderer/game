using Conesoft.Engine.ResourceLoader;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Conesoft.Engine.Resources
{
    public interface IResource<T> : IEnumerable<KeyValuePair<string, T>>
    {
        T this[string Name] { get; }

        void Add(string Name, T item);

        void Add(IEnumerable<KeyValuePair<string, T>> items);

        void Rename(string from, string to);
    }

    interface IResources
    {
        IResource<Model> Models { get; }
        IResource<TerrainModel> Terrains { get; }
        IResource<Texture2D> Sprites { get; }
        IResource<SoundEffect> Sounds { get; }
        IResource<SpriteFont> Fonts { get; }

        void Load(Action<IResources, IResourceLoader> action);
    }

    namespace Implementation
    {
        class Resource<T> : IResource<T>
        {
            Dictionary<string, T> resources;

            public Resource()
            {
                resources = new Dictionary<string, T>();
            }

            public T this[string Name]
            {
                get { return resources[Name]; }
            }

            public void Add(string Name, T item)
            {
                resources[Name] = item;
            }

            private void Add(KeyValuePair<string, T> item)
            {
                resources[item.Key] = item.Value;
            }

            public void Add(IEnumerable<KeyValuePair<string, T>> items)
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }

            public void Rename(string from, string to)
            {
                resources[to] = resources[from];
                resources.Remove(from);
            }

            public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
            {
                return resources.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return resources.GetEnumerator();
            }
        }

        class Resources : IResources
        {
            public IResource<Model> Models { get; private set; }
            public IResource<TerrainModel> Terrains { get; private set; }
            public IResource<Texture2D> Sprites { get; private set; }
            public IResource<SoundEffect> Sounds { get; private set; }
            public IResource<SpriteFont> Fonts { get; private set; }

            private IResourceLoader resourceLoader;

            public Resources(IResourceLoader resourceLoader)
            {
                Models = new Resource<Model>();
                Terrains = new Resource<TerrainModel>();
                Sprites = new Resource<Texture2D>();
                Sounds = new Resource<SoundEffect>();
                Fonts = new Resource<SpriteFont>();

                this.resourceLoader = resourceLoader;
            }

            public void Load(Action<IResources, IResourceLoader> action)
            {
                action(this, this.resourceLoader);
            }
        }
    }
}
