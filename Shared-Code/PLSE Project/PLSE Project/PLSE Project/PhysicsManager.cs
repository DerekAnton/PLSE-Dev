using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace PLSE_Project
{
    class PhysicsManager
    {
        private static PhysicsManager physicsManager;
        private const float defaultGrav = 0.5f;
        private static AccelVec gravity = new AccelVec(0,defaultGrav);
        

        private PhysicsManager() {}

        public static PhysicsManager ThePhysicsManager
        {
            get
            {
                if (physicsManager == null)
                    physicsManager = new PhysicsManager();
                return physicsManager;
            }
        }

        public static void setGravity(float gravitySpeed)
        {
            gravity.setVec(0,gravitySpeed);
        }

        public static void setGravity(Vector2 gravityVec)
        {
            gravity.setVec(gravityVec);
        }

        public static void reset()
        {
            gravity.setVec(0, defaultGrav);
        }

        public static AccelVec getGravity()
        {
            return gravity;
        }
    }
}
