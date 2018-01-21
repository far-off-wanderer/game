using Microsoft.Xna.Framework;
using Windows.Foundation;

namespace Conesoft.Engine
{
    public class CameraModel
    {
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public CameraModel(Game.Camera camera, float eyeShift, Size screenSize)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(camera.FieldOFView, (float)screenSize.Width / (float)screenSize.Height, camera.NearCutOff, camera.FarCutOff);

            var right = Vector3.Normalize(Vector3.Cross(camera.Target - camera.Position, camera.Up));

            View = Matrix.CreateLookAt(camera.Position + eyeShift * right, camera.Target + eyeShift * .5f * right, camera.Up);
        }
    }
}
