using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace PLSE_Project
{
    class CameraManager
    {
        //Positioning offsets used for when a player is too close to a wall, ceiling or floor and the camera goes slightly off center from the character
        private static int xOffset = 0, yOffset = 0, xPositioningOffset, yPositioningOffset; 
        private static Rectangle viewportRect;

        private static Rectangle levelBoundryRect;


        public static int getXOffset()
        {
            return xOffset;
        }

        public static int getYOffset()
        {
            return yOffset;
        }

        public static void setYOffset(int y)
        {
            yOffset = y;
        }

        public static void setXOffset(int x)
        {
            xOffset = x;
        }

        public static void setXandYOffset(int x, int y)
        {
            xOffset = x;
            yOffset = y;
        }

        public static void changeXOffset(int amount)
        {
            xOffset += amount;
        }

        public static void changeYOffset(int amount)
        {
            yOffset += amount;
        }

        public static void addLevelRect(int x, int y, int width, int height)
        {
            levelBoundryRect = new Rectangle(x, y, width, height);
        }

        public static Rectangle getViewportRect()
        {
            return viewportRect;
        }

        public static void setViewportRect(Rectangle viewport)
        {
            viewportRect = viewport;
        }

        public static void setPositioningOffsets(int vertical, int horizontal)
        {
            xPositioningOffset = horizontal;
            yPositioningOffset = vertical;
        }

    }
}
