using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;



namespace PLSE_Project
{
    class Hero : Colideable
    {
        private Texture2D texture;
        private string imgPath = "Sprites//Hero//Sanic";
        private Rectangle rect;
        private Vector2 origin;

        private Rectangle sourceRect = new Rectangle(0,0,50,58);

        private const int maxJumpCount = 2;
        private int jumpCount = maxJumpCount;
        private const double jumpTime = 1500;
        private const float jumpSpeed = 5.0f;
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

        public void update(double elapsedTime, KeyboardState keyState, MouseState mouseState)
        {

        }

        public void draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(texture, rect, Color.White);
            spriteBatch.Draw(texture, rect, sourceRect, Color.White, MathHelper.ToRadians(180.0f), origin, SpriteEffects.None, 1);
            int y = 7;
        }


        public Vector2 getOrigin()
        {
            return origin;
        }

        private void move(KeyboardState keyState, KeyboardState oldKeyState, double elapsedTime)
        {
            //Code for which way the character should be facing will go here.

            if(keyState.IsKeyDown(Keys.W))
                jump();

            if (keyState.IsKeyDown(Keys.S) && onGround)
                crouch();
        }

        

        private void attack()
        {
        }

        private void shiftX(double amount)
        {
        }

        private void shiftY(double amount)
        {
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
            if (jumpCount > 0)
            {
                jumpCount--;
                jumpVec = new AccelVec(0, jumpSpeed, jumpTime);
                jumping = true;
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
