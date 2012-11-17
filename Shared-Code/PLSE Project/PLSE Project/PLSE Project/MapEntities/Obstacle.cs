using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
namespace PLSE_Project
{
    public enum Layer { Foreground, Background, Midground };
    class Obstacle : Colideable
    {
        private Rectangle rect;
        private Rectangle collisionRect;
        private Rectangle drawRect;
        private Texture2D texture;
        private bool onScreen;

        public Obstacle(int x, int y, string imgPath, ContentManager content)
        {
            //The following code may seem redundant but this is a very dumbed down version of loading image for
            //obstacles. Later the imgPath will probably be to a folder of images to use for one colidable object
            //and the loading of images will probably be abstraced to a load method.
            texture = content.Load<Texture2D>(imgPath);

            rect = new Rectangle(x, y, texture.Width, texture.Height);
            drawRect = new Rectangle(x, y, texture.Width, texture.Height);
        }

        public Obstacle(int x, int y, int width, int height, string imgPath, ContentManager content)
        {
            //The following code may seem redundant but this is a very dumbed down version of loading image for
            //obstacles. Later the imgPath will probably be to a folder of images to use for one colidable object
            //and the loading of images will probably be abstraced to a load method.
            texture = content.Load<Texture2D>(imgPath);

            rect = new Rectangle(x, y, width, height);
            drawRect = new Rectangle(x, y, texture.Width, texture.Height);
        }

        public bool intersects(Rectangle rectangle)
        {
            return rect.Intersects(rectangle);
        }

        public bool intersects(Colideable obj)
        {
            return rect.Intersects(obj.getRect());
        }

        public bool isOnScreen()
        {
            return onScreen;
        }

        public Rectangle getRect()
        {
            return rect;
        }

        public void update()
        {
            onScreen = CameraManager.getViewportRect().Intersects(rect) || CameraManager.getViewportRect().Contains(rect);
            CameraManager.getXOffset();
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, drawRect, Color.White);
        }        
    }
}
