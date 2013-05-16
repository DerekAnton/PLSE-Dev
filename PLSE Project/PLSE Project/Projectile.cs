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
    public enum ProjectileDirection { Up, Down, Left, Right};

    class Projectile : TransitionBodyPart
    {
        public Projectile(ContentManager content, int amountOfSheets, int[] frameAmount, Rectangle[] sourceRect, string[] imgPath, Vector2 startingPos, int[] frameDelayTimes)
            : base(content, amountOfSheets, frameAmount, sourceRect, imgPath, startingPos, frameDelayTimes)
        { }

        public bool active = false;
        public bool shotRight = true;
        public bool shotUp = false;

        public Vector2 shiftedPosition; //= new Vector2(0, 0);

        public readonly int WEIGHT = 1;
        public int rise = 15;
        public int run = 15;

        public bool archingProjectile = false;

        public bool spriteFlipping = false;

        public Rectangle hitBox;

        private BulletType bulletType;

        private int bulletDamage = 0;

        public void draw(SpriteBatch spriteBatch, bool spriteFlipping, bool difference)
        {

            if (!spriteFlipping)
                spriteBatch.Draw(spriteSheets[currentActiveSprite], shiftedPosition, sourceRect[currentActiveSprite], Color.White, 0, originVecs[currentActiveSprite], 1.0f, SpriteEffects.None, 1.0f);
            else
                spriteBatch.Draw(spriteSheets[currentActiveSprite], shiftedPosition, sourceRect[currentActiveSprite], Color.White, 0, originVecs[currentActiveSprite], 1.0f, SpriteEffects.FlipHorizontally, 1.0f);
        }



        public void setBulletType(BulletType bultype)
        {
            bulletType = bultype;
        }
        public BulletType getBulletType()
        {
            return bulletType;
        }
        public void setDmg(int dmg)
        {

        }
        public void drop()
        {
            if (rise > -30)
                rise -= WEIGHT;
        }
        public void setRiseRun(int rise, int run)
        {
            this.rise = rise;
            this.run = run;
        }
        public Rectangle getHitbox()
        {
            return hitBox;
        }

    }
}
