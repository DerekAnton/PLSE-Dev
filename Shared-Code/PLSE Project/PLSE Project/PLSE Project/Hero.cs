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
    public enum BodySpriteIndex { BodyCrouch, BodyUncrouch, BodyStillCrouch, BodyRun, BodyStill}; // order in which the sprite sheets will be stored in the Texture2D array inside of the BodyPart Obj
    public enum LegSpriteIndex { LegCrouch, LegUncrouch, LegStillCrouch, LegRun, LegStill }; // Danton //
    public enum HeroStates { StandStill, CrouchStill, CrouchWalking, Running};

    class Hero : Colideable
    {
        // staging arrays for the start of a new BodyPart Object //
        private int amountOfSheets;
        private int[] frameAmounts; 
        private Rectangle[] frameRects;
        private string[] imgPaths;
        private Vector2[] startingPos;
        private int[] spriteDelayTimes;
        private int movespeed = 5;
        public Rectangle hitbox;

        private static double health = 800;
        private static double maxHealth = 1000;
        private static double energy = 100;
        private static double maxEnergy = 100;

        // hard coded values for the starting position of the leg sprites and the body sprites
        private Vector2 legsStartPos = new Vector2(130,275);
        private Vector2 bodyStartPos = new Vector2(165,190);

        BodyPart body; // Danton //
        BodyPart legs;
        HeroStates heroState = HeroStates.StandStill;

        bool spriteFlipping = false; // Danton //
        bool crouching = false;

        Vector2 mousePosition = new Vector2(0, 0);

        private static Rectangle rect; // old rect
        private Vector2 origin; // old origin

        private const int maxJumpCount = 1;
        private int jumpCount = maxJumpCount;
        private const double jumpTime = 90;
        private const float jumpSpeed = 1.5f;
        private bool jumping = false;
        private AccelVec jumpVec;
        private bool onGround = false;

        private const float speed = 3.0f;

        private Direction facingDirection;

        public Hero() { }

        public void load(ContentManager content, int x, int y)
        {
            initalizeStagingArrays();

            setBodyHardCodedVals();
            body = new BodyPart(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);
            
            setLegHardCodedVals();
            legs = new BodyPart(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);
            
            body.setCurrentActiveSprite((int)BodySpriteIndex.BodyStill);
            legs.setCurrentActiveSprite((int)LegSpriteIndex.LegStill);

            refreshHitbox();
        }

        public void update(double elapsedTime, KeyboardState keyState, MouseState mouseState, Rectangle viewportRect, GameTime gameTime)
        {
            body.animate(gameTime);
            legs.animate(gameTime);

            move(keyState, mouseState, viewportRect, elapsedTime, gameTime);
        }

        public void draw(SpriteBatch spriteBatch)
        {
            legs.draw(spriteBatch);
            body.draw(spriteBatch);
        }
            
        public Vector2 getOrigin()
        {
            return origin;
        }

        private void move(KeyboardState keyState, MouseState mouseState, Rectangle viewportRect, double elapsedTime, GameTime gameTime)
        {
            
            if (keyState.IsKeyDown(Keys.W))
                jump();

            if (keyState.IsKeyDown(Keys.S) && (!keyState.IsKeyDown(Keys.D) || !keyState.IsKeyDown(Keys.A)) && onGround) // case for crouchstill //
            {
                crouching = true;
                heroState = HeroStates.CrouchStill;
                body.setCurrentActiveSprite((int)BodySpriteIndex.BodyStillCrouch);
                legs.setCurrentActiveSprite((int)LegSpriteIndex.LegStillCrouch);
                refreshHitbox();
            }

            if (keyState.IsKeyDown(Keys.S) && (keyState.IsKeyDown(Keys.D) || keyState.IsKeyDown(Keys.A))) // case for crouch walking //
            {
                crouching = true;
                heroState = HeroStates.CrouchWalking;
                body.setCurrentActiveSprite((int)BodySpriteIndex.BodyStillCrouch);
                legs.setCurrentActiveSprite((int)LegSpriteIndex.LegStillCrouch);
                refreshHitbox();
            }

            if (keyState.IsKeyUp(Keys.S) && ((!keyState.IsKeyDown(Keys.D) || !keyState.IsKeyDown(Keys.A))) && onGround) // case for standing still //
            {
                crouching = false;
                heroState = HeroStates.StandStill;
                body.setCurrentActiveSprite((int)BodySpriteIndex.BodyStill);
                legs.setCurrentActiveSprite((int)LegSpriteIndex.LegStill);
                refreshHitbox();
            }

            if (keyState.IsKeyDown(Keys.D)) // case for walking right & crouch walking //
            {
                if (keyState.IsKeyDown(Keys.S))
                {
                    heroState = HeroStates.CrouchWalking;
                    moveRight(gameTime);
                    body.setCurrentActiveSprite((int)BodySpriteIndex.BodyStillCrouch);
                    legs.setCurrentActiveSprite((int)LegSpriteIndex.LegStillCrouch);
                    refreshHitbox();
                }
                else
                {
                    heroState = HeroStates.Running;
                    moveRight(gameTime);
                    body.setCurrentActiveSprite((int)BodySpriteIndex.BodyRun);
                    legs.setCurrentActiveSprite((int)LegSpriteIndex.LegRun);
                    refreshHitbox();
                }
            }
            if (keyState.IsKeyDown(Keys.A)) // case for walking left and crouching //
            {
                if (keyState.IsKeyDown(Keys.S))
                {
                    heroState = HeroStates.CrouchWalking;
                    moveLeft(gameTime);
                    body.setCurrentActiveSprite((int)BodySpriteIndex.BodyStillCrouch);
                    legs.setCurrentActiveSprite((int)LegSpriteIndex.LegStillCrouch);
                    refreshHitbox();
                }
                else
                {
                    heroState = HeroStates.Running;
                    moveLeft(gameTime);
                    body.setCurrentActiveSprite((int)BodySpriteIndex.BodyRun);
                    legs.setCurrentActiveSprite((int)LegSpriteIndex.LegRun);
                    refreshHitbox();
                }
            }

            fallCheck(viewportRect, elapsedTime);

            if (jumping)
                shiftY(-jumpVec.getShiftY(), elapsedTime);

            if (onGround)
            {
                jumpCount = maxJumpCount;
            }
        }

        private void fallCheck(Rectangle viewportRect, double elapsedTime)
        {
            if (hitbox.Bottom >= viewportRect.Bottom)
            {
                body.position[body.getCurrentActiveSprite()].Y -= hitbox.Bottom - viewportRect.Bottom;
                legs.position[body.getCurrentActiveSprite()].Y -= hitbox.Bottom - viewportRect.Bottom;
                refreshHitbox();
                onGround = true;
            }
            else
                onGround = false;
            //Gravity if it has a horizontal component will not be stopped by jumping
            //shiftX(PhysicsManager.getGravity().getShiftX(), elapsedTime);
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
            //rect.Y += (int)(amount * elapsedTime);
            //origin = new Vector2(rect.Center.X, rect.Center.Y);

            body.position[body.getCurrentActiveSprite()].Y += (int)(amount * elapsedTime);
            legs.position[body.getCurrentActiveSprite()].Y += (int)(amount * elapsedTime);
            refreshHitbox();
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
        private void moveRight(GameTime gameTime) // Danton //
        {
            if (!crouching)
            {
                body.moveAllLateral(movespeed);
                legs.moveAllLateral(movespeed);
            }
            else
            {
                body.moveAllLateral(movespeed/2);
                legs.moveAllLateral(movespeed/2);
            }
        }
        private void moveLeft(GameTime gameTime) // Danton //
        {
            if (!crouching)
            {
                body.moveAllLateral(-movespeed);
                legs.moveAllLateral(-movespeed);
            }
            else
            {
                body.moveAllLateral(-movespeed / 2);
                legs.moveAllLateral(-movespeed / 2);
            }
        }


        public Rectangle calcNewHitbox(Rectangle bodyBoundingRect, Rectangle legsBoundingRect, Vector2 position) // hitbox refresh logic //
        {
            int newWidth = bodyBoundingRect.Width > legsBoundingRect.Width ? bodyBoundingRect.Width : legsBoundingRect.Width;
            int newHeight = bodyBoundingRect.Height > legsBoundingRect.Height ? bodyBoundingRect.Height : legsBoundingRect.Height;

            return new Rectangle((int)position.X, (int)position.Y, newWidth, newHeight);
        }
        private void refreshHitbox()
        {
            hitbox = calcNewHitbox(body.sourceRect[body.getCurrentActiveSprite()], legs.sourceRect[legs.getCurrentActiveSprite()], body.position[body.getCurrentActiveSprite()]);
        }

        private void setBodyHardCodedVals() // Danton //
        {
            amountOfSheets = 5;

            imgPaths[(int)BodySpriteIndex.BodyCrouch] = "Sprites//Hero//ADV RIFLE//adv_rifle_crouch";
            imgPaths[(int)BodySpriteIndex.BodyUncrouch] = "Sprites//Hero//ADV RIFLE//adv_rifle_uncrouch";
            imgPaths[(int)BodySpriteIndex.BodyStillCrouch] = "Sprites//Hero//ADV RIFLE//adv_rifle_crouchidle";
            imgPaths[(int)BodySpriteIndex.BodyRun] = "Sprites//Hero//ADV RIFLE//adv_rifle_walking";
            imgPaths[(int)BodySpriteIndex.BodyStill] = "Sprites//Hero//ADV RIFLE//adv_rifle_idle";
            
            frameAmounts[(int)BodySpriteIndex.BodyCrouch] = 4; // these are not implemented yet [NOTE] ALL BODY CROUCH/UNCROUCH LEG CROUCH/UNCROUCH ARE NOT IMPLEMENTED YET THEY ARE FILLED WITH DUMMY VALS //
            frameAmounts[(int)BodySpriteIndex.BodyUncrouch] = 4; // these are not implemented yet
            frameAmounts[(int)BodySpriteIndex.BodyStillCrouch] = 25;
            frameAmounts[(int)BodySpriteIndex.BodyRun] = 23;
            frameAmounts[(int)BodySpriteIndex.BodyStill] = 25;

           /* frameRects[(int)BodySpriteIndex.BodyCrouch] = new Rectangle(0, 0, 118, 166);
            frameRects[(int)BodySpriteIndex.BodyUncrouch] = new Rectangle(0, 0, 118, 166);
            frameRects[(int)BodySpriteIndex.BodyStillCrouch] = new Rectangle(0, 0, 118, 166);
            frameRects[(int)BodySpriteIndex.BodyRun] = new Rectangle(0, 0, 109, 191);
            frameRects[(int)BodySpriteIndex.BodyStill] = new Rectangle(0, 0, 109, 192);
            * */

            for (int counter = 0; counter < amountOfSheets; counter++)
            {
                frameRects[counter] = new Rectangle(0,0,141,166);
                spriteDelayTimes[counter] = 50;
            }

            startingPos[(int)BodySpriteIndex.BodyCrouch] = new Vector2(bodyStartPos.X, bodyStartPos.Y);
            startingPos[(int)BodySpriteIndex.BodyUncrouch] = new Vector2(bodyStartPos.X, bodyStartPos.Y);
            startingPos[(int)BodySpriteIndex.BodyStillCrouch] = new Vector2(bodyStartPos.X, bodyStartPos.Y);
            startingPos[(int)BodySpriteIndex.BodyRun] = new Vector2(bodyStartPos.X, bodyStartPos.Y);
            startingPos[(int)BodySpriteIndex.BodyStill] = new Vector2(bodyStartPos.X, bodyStartPos.Y);

            /*
            spriteDelayTimes[(int)BodySpriteIndex.BodyCrouch] = 50;
            spriteDelayTimes[(int)BodySpriteIndex.BodyUncrouch] = 50;
            spriteDelayTimes[(int)BodySpriteIndex.BodyStillCrouch] = 50;
            spriteDelayTimes[(int)BodySpriteIndex.BodyRun] = 50;
            spriteDelayTimes[(int)BodySpriteIndex.BodyStill] = 50;
             * */
            
        }
        private void setLegHardCodedVals() // Danton //
        {
            amountOfSheets = 5; // this is for the TOTAL amount of sheets that wil be loaded (this number will obviously increase over time as different animations will be needed) //

            imgPaths[(int)LegSpriteIndex.LegCrouch] = "Sprites//Hero//LEGS//crouch_";
            imgPaths[(int)LegSpriteIndex.LegUncrouch] = "Sprites//Hero//LEGS//uncrouch";
            imgPaths[(int)LegSpriteIndex.LegStillCrouch] = "Sprites//Hero//LEGS//crouchwalk";
            imgPaths[(int)LegSpriteIndex.LegRun] = "Sprites//Hero//LEGS//running";
            imgPaths[(int)LegSpriteIndex.LegStill] = "Sprites//Hero//LEGS//idle";

            frameAmounts[(int)LegSpriteIndex.LegCrouch]= 4;
            frameAmounts[(int)LegSpriteIndex.LegUncrouch] = 4;
            frameAmounts[(int)LegSpriteIndex.LegStillCrouch] = 1;
            frameAmounts[(int)LegSpriteIndex.LegRun] = 24;
            frameAmounts[(int)LegSpriteIndex.LegStill] = 25;

            /*frameRects[(int)LegSpriteIndex.LegCrouch] = new Rectangle(0, 0, 145, 125); 
            frameRects[(int)LegSpriteIndex.LegUncrouch] = new Rectangle(0, 0, 145, 125);
            frameRects[(int)LegSpriteIndex.LegStillCrouch] = new Rectangle(0, 0, 52, 27);
            frameRects[(int)LegSpriteIndex.LegRun] = new Rectangle(0, 0, 97, 119);
            frameRects[(int)LegSpriteIndex.LegStill] = new Rectangle(0, 0, 23, 63);
            */
            for (int counter = 0; counter < amountOfSheets; counter++)
            {
                frameRects[counter] = new Rectangle(0, 0, 101, 86);
                spriteDelayTimes[counter] = 50;
            }

            startingPos[(int)LegSpriteIndex.LegCrouch] = new Vector2(legsStartPos.X,legsStartPos.Y);
            startingPos[(int)LegSpriteIndex.LegUncrouch] = new Vector2(legsStartPos.X, legsStartPos.Y);
            startingPos[(int)LegSpriteIndex.LegStillCrouch] = new Vector2(legsStartPos.X, legsStartPos.Y);
            startingPos[(int)LegSpriteIndex.LegRun] = new Vector2(legsStartPos.X, legsStartPos.Y);
            startingPos[(int)LegSpriteIndex.LegStill] = new Vector2(legsStartPos.X, legsStartPos.Y);

            /*
            spriteDelayTimes[(int)LegSpriteIndex.LegCrouch] = 50;
            spriteDelayTimes[(int)LegSpriteIndex.LegUncrouch] = 50;
            spriteDelayTimes[(int)LegSpriteIndex.LegStillCrouch] = 50;
            spriteDelayTimes[(int)LegSpriteIndex.LegRun] = 50;
            spriteDelayTimes[(int)LegSpriteIndex.LegStill] = 50;
             */
        }

        private void initalizeStagingArrays()
        {
            frameAmounts = new int[5]; 
            frameRects = new Rectangle[5];
            imgPaths = new string[5];
            startingPos = new Vector2[5];
            spriteDelayTimes = new int[5];
        }

        public static void setX(int x)
        {
            rect.X = x;
        }

        public static void setY(int y)
        {
            rect.Y = y;
        }

        public static double getHealth()
        {
            return health;
        }

        public static double getMaxHealth()
        {
            return maxHealth;
        }

        public static double getEnergy()
        {
            return energy;
        }

        public static double getMaxEnergy()
        {
            return maxEnergy;
        }

        public static void changeEnergy(double changeAmount)
        {
            energy += changeAmount;
            if (energy > maxEnergy)
                energy = maxEnergy;
        }
    }
}
