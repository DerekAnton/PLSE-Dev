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
    class Hero
    {
        Texture2D texture;
        string imgPath = "Sprites//Hero//Sanic";
        Rectangle rect;

        public Hero(){}

        public void load(ContentManager content)
        {
            texture = content.Load<Texture2D>(imgPath);
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
    }
}
