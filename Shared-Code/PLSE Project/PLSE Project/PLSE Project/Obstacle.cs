using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
namespace PLSE_Project
{
    class Obstacle : Colideable
    {
        private Rectangle rect;
        private Texture2D texture;
        private string imagePath;

        public Obstacle(int x, int y, string imgPath, ContentManager content)
        {
            //The following code may seem redundant but this is a very dumbed down version of loading image for
            //obstacles. Later the imgPath will probably be to a folder of images to use for one colidable object
            //and the loading of images will probably be abstraced to a load method.
            imagePath = imgPath;
            texture = content.Load<Texture2D>(imagePath);

            rect = new Rectangle(x, y, texture.Width, texture.Height);
        }

        public bool intersects(Rectangle rectangle)
        {
            return rect.Intersects(rectangle);
        }

        public bool intersects(Colideable obj)
        {
            return rect.Intersects(obj.getRect());
        }

        public Rectangle getRect()
        {
            return rect;
        }

    }
}
