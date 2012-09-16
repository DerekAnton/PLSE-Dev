using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace PLSE_Project
{
    class AccelVec
    {
        private Vector2 vec;
        private double decayTimer = 0;
        private bool active = false;

        //For forces that have a time duration that will eventually stop
        public AccelVec(float shiftX, float shiftY, double decayTime)
        {
            active = true;
            vec = new Vector2(shiftX, shiftY);
            decayTimer = decayTime;
        }

        //For forces that will be constantly applied ex: gravity
        public AccelVec(float shiftX, float shiftY)
        {
            active = true;
            vec = new Vector2(shiftX, shiftY);
            decayTimer = 0;
        }

        public AccelVec(Vector2 accelerationVector)
        {
            active = true;
            vec = accelerationVector;
            decayTimer = 0;
        }

        public float getShiftX()
        {
            return vec.X;
        }

        public float getShiftY()
        {
            return vec.Y;
        }

        public void update(double elapsedTime)
        {
            decayTimer -= elapsedTime;
            active = decayTimer > 0;
            Console.WriteLine(decayTimer + " : " + active);
        }

        public bool isActive()
        {
            return active;
        }

        public void setVec(float X, float Y)
        {
            vec = new Vector2(X, Y);
        }

        public void setVec(Vector2 newVec)
        {
            vec = newVec;
        }
    }
}
