using System;

namespace Conesoft.Engine.Level
{
    public interface ILevel
    {
        void Update(TimeSpan timeSpan);
        void Draw();
    }
}
