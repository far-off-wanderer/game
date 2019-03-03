using Microsoft.Xna.Framework.Graphics;

namespace Far_Off_Wanderer
{
    public class Content
    {
        private readonly GlobalContent content;
        private readonly string sceneName;

        public Content(GlobalContent content, string sceneName)
        {
            this.content = content;
            this.sceneName = sceneName;
        }

        public T Get<T>(string name) => content.Get<T>(name, sceneName);

        public IndexBuffer CreateIndexBuffer<T>(T[] indicees) where T : struct => content.CreateIndexBuffer(indicees);

        public VertexBuffer CreateVertexBuffer<T>(T[] vertices) where T : struct => content.CreateVertexBuffer(vertices);
    }
}