using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PLSE_Project.Camera
{
    class Boundry
    {
        public enum BoundryType {Left, Right, Top, Bottom };

        Rectangle rect, shiftedRect;
        bool onScreen = false;
        BoundryType boundryType;

        int verticalOverlap = 0; //Should always be posative for right and bottom offset, negative for left and top offset
        int horizontalOverlap = 0;

        public Boundry(int x, int y, int width, int height, BoundryType boundryType)
        {
            rect = new Rectangle(x, y, width, height);
            shiftedRect = new Rectangle(rect.X + CameraManager.getXOffset(), rect.Y + CameraManager.getYOffset(), width, height);
            this.boundryType = boundryType;
        }

        public void update()
        {
            shiftedRect.X = rect.X + CameraManager.getXOffset();
            shiftedRect.Y = rect.Y + CameraManager.getYOffset();

            verticalOverlap = 0;
            horizontalOverlap = 0;

            if (CameraManager.getViewportRect().Intersects(shiftedRect))
            {
                switch (boundryType)
                {
                    case BoundryType.Bottom:
                        verticalOverlap = CameraManager.getViewportRect().Bottom - shiftedRect.Top;
                        break;
                    case BoundryType.Left:
                        horizontalOverlap = CameraManager.getViewportRect().Left - shiftedRect.Right;
                        break;
                    case BoundryType.Top:
                        verticalOverlap = CameraManager.getViewportRect().Top - shiftedRect.Bottom;
                        break;
                    case BoundryType.Right:
                        horizontalOverlap = CameraManager.getViewportRect().Right - shiftedRect.Left;
                        break;
                    default:
                        break;
                }
            }

            CameraManager.setPositioningOffsets(verticalOverlap, horizontalOverlap);
        }
    }
}
