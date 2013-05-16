using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace PLSE_Project
{
    class GroundPatroller : Enemy
    {
        private int moveSpeed = 1;
        private bool isOnScreen = false;

        private bool onGround;

        private Rectangle originalRect, shiftedRect;

        private enum EnemyAnimation { Die, Explode, LeftToRight, RightToLeft, WalkLeft, WalkRight };
        private enum Movement { Still, Left, Right };

        private EnemyAnimation animation = EnemyAnimation.WalkLeft;

        private bool dead = false;

        private Animation die, explode, leftToRight, rightToLeft, walkLeft, walkRight, currentAnimation;

        private int health, maxHealth = 60;

        //Texture2D testingTexture;

        public GroundPatroller(int x, int y, ContentManager content)
        {
            rightToLeft = new Animation(160, 105, content, "Sprites\\Enemies\\GroundPatroller\\righttoleft", 10, false, 30);
            leftToRight = new Animation(160, 105, content, "Sprites\\Enemies\\GroundPatroller\\lefttoright", 10, false, 30);
            die = new Animation(158, 105, content, "Sprites\\Enemies\\GroundPatroller\\die", 23, false, 30);
            explode = new Animation(158, 105, content, "Sprites\\Enemies\\GroundPatroller\\explode", 11, false, 30);
            walkLeft = new Animation(158, 105, content, "Sprites\\Enemies\\GroundPatroller\\walkleft", 17, true, 30);
            walkRight = new Animation(158, 105, content, "Sprites\\Enemies\\GroundPatroller\\walkright", 17, true, 30);

            originalRect = new Rectangle(x, y, 160, 105);
            shiftedRect = originalRect;
            currentAnimation = walkRight;
            animation = EnemyAnimation.WalkRight;

            health = maxHealth;

            //testingTexture = content.Load<Texture2D>("Sprites\\rectangle");
        }

        public void update(double elapsedTime)
        {
            shiftedRect.X = originalRect.X + CameraManager.getXOffset();
            shiftedRect.Y = originalRect.Y + CameraManager.getYOffset();

            isOnScreen = CameraManager.getViewportRect().Intersects(shiftedRect) || CameraManager.getViewportRect().Contains(shiftedRect);

            //ADDED CODE FOR HITING PLAYER
            if (!dead && shiftedRect.Intersects(Hero.getHeroHitbox()))
            {
                dead = true;
                Hero.dealHeroDmg(1);
                animation = EnemyAnimation.Explode;
                currentAnimation = explode;
            }


            onGround = false;

            foreach (Rectangle rect in ObstacleManager.getColisionRectangles())
            {
                onGround = onGround || rect.Intersects(shiftedRect) || rect.Contains(shiftedRect);
                //Console.Out.WriteLine(rect.Intersects(shiftedRect));
                if (rect.Intersects(shiftedRect) && (shiftedRect.Bottom - rect.Top <= 40))
                {
                    originalRect.Y -= shiftedRect.Bottom - rect.Top;
                    shiftedRect.Y = originalRect.Y + CameraManager.getYOffset();
                }

                if (rect.Top - 5 < shiftedRect.Bottom && (rect.Intersects(shiftedRect) || rect.Contains(shiftedRect)) && (animation == EnemyAnimation.WalkLeft || animation == EnemyAnimation.WalkRight))
                {
                    switchDirections();
                }

            }

            switch (animation)
            {
                case EnemyAnimation.WalkLeft:
                    move(Movement.Left);
                    walkLeft.update(elapsedTime);
                    break;
                case EnemyAnimation.WalkRight:
                    move(Movement.Right);
                    walkRight.update(elapsedTime);
                    break;
                case EnemyAnimation.LeftToRight:
                    leftToRight.update(elapsedTime);
                    if (leftToRight.finishedAnimation())
                    {
                        leftToRight.reset();
                        currentAnimation = walkRight;
                        animation = EnemyAnimation.WalkRight;
                    }
                    break;
                case EnemyAnimation.RightToLeft:
                    rightToLeft.update(elapsedTime);
                    if (rightToLeft.finishedAnimation())
                    {
                        rightToLeft.reset();
                        currentAnimation = walkLeft;
                        animation = EnemyAnimation.WalkLeft;
                    }
                    break;
                case EnemyAnimation.Die:
                    die.update(elapsedTime);
                    break;
                case EnemyAnimation.Explode:
                    explode.update(elapsedTime);
                    break;
            }

            //INSERT CODE FOR FALLING HERE
            if (!onGround)
            {
                originalRect.Y += (int)(0.5 * elapsedTime);
            }

        }

        private void switchDirections()
        {
            //Console.Out.WriteLine("Swtiching Direction");
            if (animation == EnemyAnimation.WalkLeft)
            {
                walkLeft.reset();
                currentAnimation = leftToRight;
                animation = EnemyAnimation.LeftToRight;
                originalRect.X += 5;
            }
            else if (animation == EnemyAnimation.WalkRight)
            {
                walkRight.reset();
                currentAnimation = rightToLeft;
                animation = EnemyAnimation.RightToLeft;
                originalRect.X -= 5;
            }
        }
        private void move(Movement movement)
        {
            if (movement == Movement.Left)
                originalRect.X -= moveSpeed;
            else if (movement == Movement.Right)
                originalRect.X += moveSpeed;
        }

        public bool isDead()
        {
            return dead;
        }

        public bool delete()
        {
            return dead && (die.finishedAnimation() || explode.finishedAnimation());
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if(isOnScreen)
                currentAnimation.draw(spriteBatch, shiftedRect.X, shiftedRect.Y+15);

            //spriteBatch.Draw(testingTexture, shiftedRect, new Rectangle(0, 0, 160, 105), Color.White);
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
                currentAnimation = die;
                animation = EnemyAnimation.Die;
            }
        }

        public Rectangle getRect()
        {
            return shiftedRect;
        }
    }
}
