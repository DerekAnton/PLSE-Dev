using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace PLSE_Project
{
    class BloatSack : Enemy
    {
        private int flySpeedVertical = 1, flySpeedHorizontal = 3;

        private int health, maxHealth = 50;

        private bool isOnScreen = false;

        private Rectangle originalRect, shiftedRect;

        private enum EnemyAnimation { Idle, ExplodeLeft, ExplodeRight };

        private EnemyAnimation animation = EnemyAnimation.Idle;

        private bool dead = false;

        private Animation idle, explodeLeft, explodeRight, currentAnimation;

        private SpriteEffects spriteEffects = SpriteEffects.None;


        Texture2D testingTexture;
        private Vector2 heroLocation;

        public BloatSack(int x, int y, ContentManager content)
        {
            idle = new Animation(138, 120, content, "Sprites\\Enemies\\BloatSack\\idle", 21, true, 30);
            explodeLeft = new Animation(138, 120, content, "Sprites\\Enemies\\BloatSack\\explodeleft", 10, false, 30);
            explodeRight = new Animation(138, 120, content, "Sprites\\Enemies\\BloatSack\\exploderight", 10, false, 30);

            originalRect = new Rectangle(x, y, 138, 120);
            shiftedRect = new Rectangle (originalRect.X, originalRect.Y, originalRect.Width, originalRect.Height);
            currentAnimation = idle;
            animation = EnemyAnimation.Idle;

            testingTexture = content.Load<Texture2D>("Sprites\\rectangle");

            health = maxHealth;
        }

        public void update(double elapsedTime)
        {
            isOnScreen = CameraManager.getViewportRect().Intersects(shiftedRect) || CameraManager.getViewportRect().Contains(shiftedRect);

            shiftedRect.X = originalRect.X + CameraManager.getXOffset();
            shiftedRect.Y = originalRect.Y + CameraManager.getYOffset();


            heroLocation = new Vector2(CameraManager.getXOffset(), ((-1) * CameraManager.getYOffset()) - 500);
            if (isOnScreen)
            {
                move(elapsedTime);
            }

            //ADDED CODE FOR HITING PLAYER
            if (!dead && shiftedRect.Intersects(Hero.getHeroHitbox()))
            {
                dead = true;
                Hero.dealHeroDmg(1);
                animation = EnemyAnimation.ExplodeRight;
                currentAnimation = explodeRight;
            }

            switch (animation)
            {
                case EnemyAnimation.Idle:
                    idle.update(elapsedTime);
                    break;
                case EnemyAnimation.ExplodeLeft:
                    explodeLeft.update(elapsedTime);
                    break;
                case EnemyAnimation.ExplodeRight:
                    explodeRight.update(elapsedTime);
                    break;
            }
        }

        private void move(double elapsedTime)
        {            
            if (shiftedRect.Center.X > 610 &&shiftedRect.Center.X - 610 > 5)
            {
                originalRect.X -= flySpeedHorizontal;

                if (spriteEffects == SpriteEffects.FlipHorizontally)
                    spriteEffects = SpriteEffects.None;
            }
            if (shiftedRect.Center.X < 610 && 610 - shiftedRect.Center.X > 5)
            {
                originalRect.X += flySpeedHorizontal;

                if (spriteEffects == SpriteEffects.None)
                    spriteEffects = SpriteEffects.FlipHorizontally;
            }

            if (shiftedRect.Center.Y > 370 && shiftedRect.Center.Y - 370 >15)
            {
                originalRect.Y -= flySpeedVertical;
            }
            if (shiftedRect.Center.Y < 370 && 370 - shiftedRect.Center.Y >15)
            {
                originalRect.Y += flySpeedVertical;
            }
        }

        public bool isDead()
        {
            return dead;
        }

        public bool delete()
        {
            return dead && (explodeLeft.finishedAnimation() || explodeRight.finishedAnimation());
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (isOnScreen)
                currentAnimation.draw(spriteBatch, shiftedRect.X, shiftedRect.Y);

            //spriteBatch.Draw(testingTexture, new Vector2(590,350), new Rectangle(0, 0, 20, 20), Color.White);
            //Console.Out.WriteLine("X: " + shiftedRect.X + ", Y: " + shiftedRect.Y + ", Width: " + shiftedRect.Width + ", Height: " + shiftedRect.Height);
        }

        public bool intersects(Rectangle rect)
        {
            return shiftedRect.Intersects(rect);
        }

        public void doDamage(int damage)
        {
            health -= damage;
            dead = (health <= 0);

            if (dead)
            {
                currentAnimation = explodeLeft;
                animation = EnemyAnimation.ExplodeLeft;
            }
        }

        public Rectangle getRect()
        {
            return shiftedRect;
        }
    }
}
