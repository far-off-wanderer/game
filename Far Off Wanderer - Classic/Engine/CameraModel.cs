using Microsoft.Xna.Framework;
using Windows.Foundation;

namespace Conesoft.Engine
{
    public class CameraModel
    {
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public CameraModel(Game.Camera camera, Size screenSize)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(camera.FieldOFView, (float)screenSize.Width / (float)screenSize.Height, camera.NearCutOff, camera.FarCutOff);
            View = Matrix.CreateLookAt(camera.Position, camera.Target, camera.Up);
        }
    }
}
