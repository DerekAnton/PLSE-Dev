using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace PLSE_Project
{
    public enum Layer { Foreground, Background, Midground };
    class Obstacle : Colideable
    {
        private Rectangle originalRect;
        private Rectangle collisionRect;
        private Rectangle shiftedRect;
        private Texture2D texture;
        private bool onScreen;

        private double cameraScale = 1;

        public Obstacle(int x, int y, string imgPath, ContentManager content)
        {
            //The following code may seem redundant but this is a very dumbed down version of loading image for
            //obstacles. Later the imgPath will probably be to a folder of images to use for one colidable object
            //and the loading of images will probably be abstraced to a load method.
            texture = content.Load<Texture2D>(imgPath);

            originalRect = new Rectangle(x, y, texture.Width, texture.Height);
            shiftedRect = new Rectangle(x, y, texture.Width, texture.Height);
        }

        public Obstacle(int x, int y, int width, int height, string imgPath, ContentManager content)
        {
            //The following code may seem redundant but this is a very dumbed down version of loading image for
            //obstacles. Later the imgPath will probably be to a folder of images to use for one colidable object
            //and the loading of images will probably be abstraced to a load method.
            texture = content.Load<Texture2D>(imgPath);

            originalRect = new Rectangle(x, y, width, height);
            shiftedRect = new Rectangle(x, y, texture.Width, texture.Height);
        }

        public Obstacle(int x, int y, string imgPath, ContentManager content, double cameraScale)
        {
            //The following code may seem redundant but this is a very dumbed down version of loading image for
            //obstacles. Later the imgPath will probably be to a folder of images to use for one colidable object
            //and the loading of images will probably be abstraced to a load method.
            texture = content.Load<Texture2D>(imgPath);

            originalRect = new Rectangle(x, y, texture.Width, texture.Height);
            shiftedRect = new Rectangle(x, y, texture.Width, texture.Height);

            this.cameraScale = cameraScale;
        }

        public bool intersects(Rectangle rectangle)
        {
            return shiftedRect.Intersects(rectangle);
        }

        public bool intersects(Colideable obj)
        {
            return shiftedRect.Intersects(obj.getRect());
        }

        public bool isOnScreen()
        {
            return onScreen;
        }

        public Rectangle getRect()
        {
            return shiftedRect;
        }

        public void update()
        {
            shiftedRect.X = originalRect.X + (int)((double)CameraManager.getXOffset() * cameraScale);
            shiftedRect.Y = originalRect.Y + (int)((double)CameraManager.getYOffset() * cameraScale);

            onScreen = CameraManager.getViewportRect().Intersects(shiftedRect) || CameraManager.getViewportRect().Contains(shiftedRect);
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if(onScreen)
                spriteBatch.Draw(texture, shiftedRect, Color.White);
        }        
    }
}
