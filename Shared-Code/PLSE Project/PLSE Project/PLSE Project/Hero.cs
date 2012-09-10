using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;



namespace PLSE_Project
{
    class Hero : Colideable
    {
        private Texture2D texture;
        private string imgPath = "Sprites//Hero//Sanic";
        private Rectangle rect;

        public Hero(){}

        public void load(ContentManager content, int x, int y)
        {
            texture = content.Load<Texture2D>(imgPath);
            rect = new Rectangle(x, y, texture.Width, texture.Height);
        }

        public void update(double elapsedTime, KeyboardState keyState)
        {

        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, rect, Color.White);
        }


        public Vector2 getOrigin()
        {
            return new Vector2(rect.Center.X, rect.Center.Y);
        }

        private void move(KeyboardState keyState, KeyboardState oldKeyState, double elapsedTime)
        {

        }

        public void setRect(int x, int y)
        {
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
