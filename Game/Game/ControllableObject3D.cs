using Far_Off_Wanderer.Game;

namespace Far_Off_Wanderer
{
    public abstract class ControllableObject3D : Object3D
    {
        public abstract void HorizontalTurnAngle(float Angle);
        public abstract void AccelerateAmount(float Amount);
        public abstract void Strafe(StrafingDirection direction);
        public abstract void Shoot();
    }
}
