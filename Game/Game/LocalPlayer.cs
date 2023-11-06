﻿namespace Far_Off_Wanderer.Game;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using XNA = Microsoft.Xna.Framework.Input;

public class LocalPlayer : Player
{
    public static float deadZone = (float)Math.Sin(MathHelper.ToRadians(1));

    public override void UpdateThinking(TimeSpan timeSpan, Environment environment)
    {
        var actions = environment.Actions;
        var spaceShip = ControlledObject as Spaceship;

        float horizontalTurnAngle = 0;
        bool shoot = false;
        StrafingDirection? strafe = null;
        foreach (var touchPoint in XNA.Touch.TouchPanel.GetState())
        {
            if (touchPoint.Position.X < environment.ScreenSize.Width / 3)
            {
                strafe = StrafingDirection.Left;
                //horizontalTurnAngle += (float)Math.PI / 2;
            }
            else if (touchPoint.Position.X > 2 * environment.ScreenSize.Width / 3)
            {
                strafe = StrafingDirection.Right;
                //horizontalTurnAngle -= (float)Math.PI / 2;
            }
            else
            {
                shoot = true;
            }
        }

        if (actions.TurningLeft)
        {
            horizontalTurnAngle += (float)Math.PI / 5;
        }
        if (actions.TurningRight)
        {
            horizontalTurnAngle -= (float)Math.PI / 5;
        }
        if (actions.TurningUp)
        {
            horizontalTurnAngle += (float)Math.PI / 5;
        }
        if (actions.TurningDown)
        {
            horizontalTurnAngle -= (float)Math.PI / 5;
        }
        if (actions.Accelerating && !actions.Decelerating)
        {
            spaceShip.AccelerateAmount(30f);
        }
        if (!actions.Accelerating && actions.Decelerating)
        {
            spaceShip.AccelerateAmount(-30f);
        }
        if (!actions.Accelerating && !actions.Decelerating)
        {
            spaceShip.AccelerateAmount(0);
        }
        if (actions.Shooting)
        {
            shoot = true;
        }
        if (actions.StrafingLeft)
        {
            strafe = StrafingDirection.Left;
        }
        else if (actions.StrafingRight)
        {
            strafe = StrafingDirection.Right;
        }

        var pad = XNA.GamePad.GetState(PlayerIndex.One);
        if (pad.IsConnected)
        {
            if (pad.IsButtonDown(Buttons.DPadLeft))
            {
                horizontalTurnAngle += (float)Math.PI / 5;
            }
            if (pad.IsButtonDown(Buttons.DPadRight))
            {
                horizontalTurnAngle -= (float)Math.PI / 5;
            }
            if (pad.IsButtonDown(Buttons.A))
            {
                shoot = true;
            }
            var stick = pad.ThumbSticks.Left;
            horizontalTurnAngle -= Math.Sign(stick.X) * (float)Math.Pow(Math.Abs(stick.X), 3) / 2.5f;
            horizontalTurnAngle += (pad.Triggers.Left - pad.Triggers.Right) * (float)Math.PI / 5;
            if (pad.IsButtonDown(Buttons.LeftShoulder))
            {
                strafe = StrafingDirection.Left;
            }
            else if (pad.IsButtonDown(Buttons.RightShoulder))
            {
                strafe = StrafingDirection.Right;
            }
        }

        if (shoot)
        {
            spaceShip.Shoot();
        }
        if (strafe.HasValue)
        {
            spaceShip.Strafe(strafe.Value);
        }
        spaceShip.HorizontalTurnAngle(horizontalTurnAngle);
        //spaceShip.AccelerateAmount(0);
    }
}
