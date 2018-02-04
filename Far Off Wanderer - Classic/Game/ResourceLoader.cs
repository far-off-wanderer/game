using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace Far_Off_Wanderer
{
    public interface IResourceLoader
    {
        T Load<T>(string Resource);
        IEnumerable<KeyValuePair<string, T>> Load<T>(params string[] Resources);
    }

    class ResourceLoader : IResourceLoader
    {
        ContentManager manager;

        public ResourceLoader(ContentManager manager)
        {
            this.manager = manager;
        }

        public T Load<T>(string Resource)
        {
            return manager.Load<T>(Resource.Replace("/", "\\"));
        }

        public IEnumerable<KeyValuePair<string, T>> Load<T>(params string[] Resources)
        {
            foreach (var resource in Resources)
            {
                yield return new KeyValuePair<string, T>(resource, Load<T>(resource));
            }
        }

    }
}
