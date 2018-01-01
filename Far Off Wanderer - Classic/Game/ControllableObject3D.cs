using System.Collections.Generic;

namespace Conesoft.Game
{
    public abstract class ControllableObject3D : Object3D
    {
        public abstract void TurnAngle(float Angle);
        public abstract void AccelerateAmount(float Amount);
        public abstract void Shoot();
    }
}
