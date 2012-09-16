using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;




namespace PLSE_Project
{
    class Hero : Colideable
    {

        private float rotation = 0f; // Danton
        Vector2 mousePosition = new Vector2(0, 0); // Danton
        Vector2 sanicDirection = new Vector2(0, 0); // Danton
        
        private Texture2D texture;
        private string imgPath = "Sprites//Hero//Sanic";
        private Rectangle rect;
        private Vector2 origin;

        private Rectangle sourceRect = new Rectangle(0,0,50,58);

        private const int maxJumpCount = 1;
        private int jumpCount = maxJumpCount;
        private const double jumpTime = 90;
        private const float jumpSpeed = 1.5f;
        private bool jumping = false;
        private AccelVec jumpVec;
        private bool onGround = false;

        private const float speed = 3.0f;

        private Direction facingDirection;
        Vector2 aimVec;

        public Hero(){}

        public void load(ContentManager content, int x, int y)
        {
            texture = content.Load<Texture2D>(imgPath);
            rect = new Rectangle(x, y, texture.Width, texture.Height);
            origin = new Vector2(rect.Center.X, rect.Center.Y);
        }

        public void update(double elapsedTime, KeyboardState keyState, MouseState mouseState, Rectangle viewportRect)
        {
            mousePosition.X = mouseState.X; // Danton
            mousePosition.Y = mouseState.Y; // Danton
            sanicDirection = mousePosition - origin; // Danton
            sanicDirection.Normalize();  // Danton
            rotation = (float)Math.Atan2((double)sanicDirection.Y, (double)sanicDirection.X);  // Danton

            if (jumping)
            {
                jumpVec.update(elapsedTime);
                jumping = jumpVec.isActive();
            }

            move(keyState, mouseState, viewportRect, elapsedTime);
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, origin, null, Color.White, rotation, new Vector2(texture.Width / 2, texture.Height / 2), 1.0f, SpriteEffects.None, 1.0f);  // Danton
        }


        public Vector2 getOrigin()
        {
            return origin;
        }

        private void move(KeyboardState keyState, MouseState mouseState, Rectangle viewportRect, double elapsedTime)
        {
            //Code for which way the character should be facing + aiming will go here.

            if(keyState.IsKeyDown(Keys.W))
                jump();

            if (keyState.IsKeyDown(Keys.S) && onGround)
                crouch();

            fallCheck(viewportRect, elapsedTime);

            if (jumping)
                shiftY(-jumpVec.getShiftY(), elapsedTime);

            if (onGround)
            {
                //jumping = false;
                jumpCount = maxJumpCount;
            }
        }

        private void fallCheck(Rectangle viewportRect, double elapsedTime)
        {
            if (rect.Bottom >= viewportRect.Bottom)
            {
                rect.Y -= rect.Bottom - viewportRect.Bottom;
                onGround = true;
            }
            else
                onGround = false;

            //TODO: Add code to check if on top of any obstacles and change on ground accordingly


            //Gravity if it has a horizontal component will not be stopped by jumping
            shiftX(PhysicsManager.getGravity().getShiftX(), elapsedTime);
            if (!jumping && !onGround)
                shiftY(PhysicsManager.getGravity().getShiftY(), elapsedTime);
        }

        private void attack()
        {
        }

        private void shiftX(float amount, double elapsedTime)
        {
            rect.X += (int)(amount * elapsedTime);
            origin = new Vector2(rect.Center.X, rect.Center.Y);
        }

        private void shiftY(float amount, double elapsedTime)
        {
            rect.Y += (int)(amount * elapsedTime);
            origin = new Vector2(rect.Center.X, rect.Center.Y);
        }

        private void setAimDirection(KeyboardState keyState, MouseState mouseState)
        {
            if (mouseState.X >= origin.X)
                facingDirection = Direction.Right;
            else
                facingDirection = Direction.Left;

            float distX = mouseState.X - origin.X;
            float distY = origin.Y - mouseState.Y;

            aimVec = new Vector2(distX / (distX + distY), distY / (distX + distY));
        }

        private void jump()
        {
            if (jumpCount > 0 && !jumping)
            {
                jumpCount--;
                jumpVec = new AccelVec(0, jumpSpeed, jumpTime);
                jumping = true;
                Console.WriteLine("JumpCount: " + jumpCount);
            }
        }

        private void crouch()
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
