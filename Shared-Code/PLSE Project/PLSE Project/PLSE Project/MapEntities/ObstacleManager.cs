using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PLSE_Project
{
    class ObstacleManager
    {
        private static LinkedList<Obstacle> foreground, midground, background;
        private static LinkedList<Rectangle> collisionRectsLoaded;
        private static Rectangle[] collisionRectsOrignal, collisionRectsShifted;

        public static void addObstacle(ContentManager content, string imgPath, int x, int y, string layer)
        {
            switch (layer)
            {
                case "Foreground":
                    foreground.AddLast(new Obstacle(x, y, imgPath, content));
                    break;
                case "Midground":
                    midground.AddLast(new Obstacle(x, y, imgPath, content));
                    break;
                case "Background":
                    background.AddLast(new Obstacle(x, y, imgPath, content));
                    break;
                default:
                    break;
            }
        }

        public static void finishedLoading()
        {
            collisionRectsOrignal = collisionRectsLoaded.ToArray<Rectangle>();
            collisionRectsLoaded = null;

            collisionRectsShifted = collisionRectsOrignal;
        }


        public static void update()
        {
            foreach (Obstacle ob in foreground)
                ob.update();
            foreach (Obstacle ob in midground)
                ob.update();
            foreach (Obstacle ob in background)
                ob.update();

            for(int i=0; i < collisionRectsShifted.Length; i++)
            {
                collisionRectsShifted[i].X = collisionRectsOrignal[i].X + CameraManager.getXOffset();
                collisionRectsShifted[i].Y = collisionRectsOrignal[i].Y + CameraManager.getYOffset();
            }
        }

        public static void drawForeground(SpriteBatch spriteBatch)
        {
            foreach (Obstacle ob in foreground)
            {
                if(ob.isOnScreen())
                ob.draw(spriteBatch);
            }
        }

        public static void drawMidground(SpriteBatch spriteBatch)
        {
            foreach (Obstacle ob in foreground)
            {
                if (ob.isOnScreen())
                    ob.draw(spriteBatch);
            }
        }

        public static void drawBackground(SpriteBatch spriteBatch)
        {
            foreach (Obstacle ob in background)
            {
                if (ob.isOnScreen())
                    ob.draw(spriteBatch);
            }
        }

        public static void addCollisionRectangle(int x, int y, int width, int height)
        {
            collisionRectsLoaded.AddFirst(new Rectangle(x, y, width, height));
        }
        public static void addCollisionRectangle(Rectangle rect)
        {
            collisionRectsLoaded.AddFirst(rect);
        }

        public static Rectangle[] getColisionObstacles()
        {
            return collisionRectsShifted;
        }

        public static void reset()
        {
            collisionRectsShifted = collisionRectsOrignal;
        }
    }
}
