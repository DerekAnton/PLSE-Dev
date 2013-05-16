using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace PLSE_Project
{
    interface Enemy
    {
        void update(double elapsedTime);
        void draw(SpriteBatch spriteBatch);
        Rectangle getRect();
        bool delete();
        bool isDead();
        bool intersects(Rectangle rect);
        void doDamage(int damage);
    }
}
