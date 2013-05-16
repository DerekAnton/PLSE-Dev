using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace PLSE_Project
{
    class Platform : Colideable
    {
        private Texture2D platform;
        private Rectangle collisionRect;

        public Platform(ContentManager content, string suffix)
        {
            platform = content.Load<Texture2D>("Sprites//" + suffix);
            if(suffix.Equals("platform"))
                collisionRect = new Rectangle(0, 400, platform.Width, platform.Height);
            else
                collisionRect = new Rectangle(400, 250, platform.Width, platform.Height);
        }

        public bool intersects(Rectangle rect) { return collisionRect.Intersects(rect); }
        public bool intersects(Colideable obj) { return obj.intersects(collisionRect); }
        public Rectangle getRect() { return collisionRect; }

        public void update()
        {

        }
        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(platform, collisionRect, Color.White);
        }

    }
}
