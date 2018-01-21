using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using Windows.UI.ViewManagement;

namespace Conesoft.Game
{
    public class LocalPlayer : Player
    {
        public static float deadZone = (float)Math.Sin(MathHelper.ToRadians(1));

        public override void UpdateThinking(TimeSpan timeSpan, DefaultEnvironment environment)
        {
            float turnAngle = 0;
            bool shoot = false;
            StrafingDirection? strafe = null;
            foreach (var touchPoint in TouchPanel.GetState())
            {
                if (touchPoint.Position.X < environment.ScreenSize.Width / 3)
                {
                    strafe = StrafingDirection.Left;
//                    turnAngle += (float)Math.PI / 2;
                }
                else if (touchPoint.Position.X > 2 * environment.ScreenSize.Width / 3)
                {
                    strafe = StrafingDirection.Right;
//                    turnAngle -= (float)Math.PI / 2;
                }
                else
                {
                    shoot = true;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                turnAngle += (float)Math.PI / 5;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                turnAngle -= (float)Math.PI / 5;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                shoot = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                strafe = StrafingDirection.Left;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                strafe = StrafingDirection.Right;
            }

            var pad = GamePad.GetState(PlayerIndex.One);
            if (pad.IsConnected)
            {
                if (pad.IsButtonDown(Buttons.DPadLeft))
                {
                    turnAngle += (float)Math.PI / 5;
                }
                if (pad.IsButtonDown(Buttons.DPadRight))
                {
                    turnAngle -= (float)Math.PI / 5;
                }
                if (pad.IsButtonDown(Buttons.A))
                {
                    shoot = true;
                }
                var stick = pad.ThumbSticks.Left.X;
                turnAngle -= Math.Sign(stick) * (float)Math.Pow(Math.Abs(stick), 3) / 2.5f;
                turnAngle += (pad.Triggers.Left - pad.Triggers.Right) * (float)Math.PI / 5;
                if (pad.IsButtonDown(Buttons.LeftShoulder))
                {
                    strafe = StrafingDirection.Left;
                }
                else if (pad.IsButtonDown(Buttons.RightShoulder))
                {
                    strafe = StrafingDirection.Right;
                }
            }

            if (UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Touch && pad.IsConnected == false)
            {
                turnAngle += Math.Sign(-environment.Acceleration.X) * MathHelper.Clamp((Math.Abs(environment.Acceleration.X) - deadZone) / (1 - deadZone), 0, 1);
            }

            var spaceShip = ControlledObject as Spaceship;
            if (shoot)
            {
                spaceShip.Shoot();
            }
            if(strafe.HasValue)
            {
                spaceShip.Strafe(strafe.Value);
            }
            spaceShip.TurnAngle(turnAngle);
            //spaceShip.AccelerateAmount(0);
        }
    }
}
