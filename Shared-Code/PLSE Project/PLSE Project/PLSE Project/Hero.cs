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



namespace PLSE_Project.Interfaces
{
    public enum BodySpriteIndex { CrouchIdle, CrouchMagic, CrouchReload, CrouchShoot, Fall, GetHurt, HangIdle, Idle, IdleUp, Jump, Magic, MagicDown, MagicUp, PullTowards, PushAway, ReloadUp, Shoot, ShootDown, ShootUp }; 
    public enum LegSpriteIndex { CrouchWalk, Fall, GetHurt, Idle, Jump, Running, CrouchIdle  }; 

    public enum BodyTransitionIndex { Crouch, CrouchHolster, CrouchTurn, /*CrouchUnHolster,*/ Hoist, Holster, LowToMid, MidToLow, MidToUp, Turn, TurnDown, TurnUp, UnCrouch, /*UnHolster,*/ UpToMid, NULL };
    public enum LegTransitionIndex { Crouch, EndRun, FallTurn, JumpTurn, StartRun, Turn, UnCrouch, NULL };

    public enum HeroStates { StandStill, CrouchStill, CrouchWalking, Running, Jumping};

    class Hero : Colideable
    {

        private readonly int MILLISECOND_DELAY = 30; // used for frame limiting 

        private Platform floor; // for debugging jumping & collision only (test harness before i recieve Jordan's level loading)//
        private Platform obstacle;
        // STAGING ARRAYS //
        private Vector2 startingPos; 
        private int[] spriteDelayTimes; // staging arrays that are filled with essential information for sprite loading thbat is delegated to each body part / transitional body part object //
        private int[] frameAmounts; 
        private Rectangle[] frameRects; 
        private string[] imgPaths;

        private const float speed = 3.0f;
        private Vector2 legsStartPos = new Vector2(115, 270);
        private Vector2 bodyStartPos = new Vector2(165, 190);
        private int movespeed = 5;
        private int amountOfSheets; 
        private string bodyString = ""; 
        private string legString = "";
        // JUMPING VARIABLES //
        private bool airborn = false;
        private bool doubleJumping = false;
        private bool falling = false;
        private int jumpAcceleration = 0;
        private int horizontalJumpInertia = 0;
        private int frameLimiter = 0;
        private int jumpsLeft = 2;

        public int legOffset = 0;
        public int bodyOffset = 0;
        public Rectangle standingHitbox = new Rectangle(50, 50, 70, 145);
        public Rectangle crouchingHitbox;

        public BodyPart body; 
        public BodyPart legs;
        BodyPart singleSheet; // the single sheet body part is important for crouching & hanging since the legs/body are one full piece //
        public TransitionBodyPart bodyTransitions; // the transition body part will break up the sprite sheets that have to do with changing a state i.e. from idle to crouching needs an quick animation played over the hero to transition it smoothly.//
        public TransitionBodyPart legsTransitions;

        HeroStates heroState = HeroStates.StandStill;

        static public LegTransitionIndex currentLegTransition = LegTransitionIndex.NULL; // these two values will be stored with the corresponding int val of whatever transition animation needs to be played //
        static public BodyTransitionIndex currentBodyTransition = BodyTransitionIndex.NULL; // //
        static public bool singleAnimationLock = true;
       
        bool spriteFlipping = false; 
        bool crouching = false;

        private bool onGround = true;
        private bool isStandingHitbox = true; // only two types of hit boxes, standing and crouching, (crouching == !standing) //
        private bool offsetCheck = false;
        private bool facingRight = true;
        bool facingUp = false;

        public Hero() { }

        public void load(ContentManager content, int x, int y)
        {
            initalizeStagingArrays();

            setBodyHardCodedVals();
            body = new BodyPart(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);
            
            setLegHardCodedVals();
            legs = new BodyPart(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);

            setSingleSheetHardCodedVals();
            singleSheet = new BodyPart(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);

            setBodyTransitionHardCodedValues();
            bodyTransitions = new TransitionBodyPart(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);

            setLegsTransitionHardCodedValues();
            legsTransitions = new TransitionBodyPart(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);

            body.setCurrentActiveSprite((int)BodySpriteIndex.Idle);
            legs.setCurrentActiveSprite((int)LegSpriteIndex.Idle);
        }

        public void update(double elapsedTime, KeyboardState keyState, KeyboardState oldKeyState, Rectangle viewportRect, GameTime gameTime)
        {
            handleFalling(gameTime);
        
            if (!(currentBodyTransition == BodyTransitionIndex.NULL))
            {
                bodyTransitions.animateUntilEndFrame(gameTime);

                if (!(currentLegTransition == LegTransitionIndex.NULL))
                    legsTransitions.animateUntilEndFrame(gameTime);
                else
                    legs.animate(gameTime);
            }
            else if (!(currentLegTransition == LegTransitionIndex.NULL))
            {
                legsTransitions.animateUntilEndFrame(gameTime);

                if (!(currentBodyTransition == BodyTransitionIndex.NULL))
                    bodyTransitions.animateUntilEndFrame(gameTime);
                else
                    body.animate(gameTime);
            }
            else
            {
                body.animate(gameTime);
                legs.animate(gameTime);
            }
            move(keyState, oldKeyState, viewportRect, elapsedTime, gameTime);
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (spriteFlipping)
            {

                if (!(currentBodyTransition == BodyTransitionIndex.NULL))
                {
                    if (!(currentLegTransition == LegTransitionIndex.NULL))
                        legsTransitions.draw(spriteBatch, spriteFlipping);
                    else
                        legs.draw(spriteBatch, spriteFlipping);

                    bodyTransitions.draw(spriteBatch, spriteFlipping);

                }
                else if (!(currentLegTransition == LegTransitionIndex.NULL))
                {
                    legsTransitions.draw(spriteBatch, spriteFlipping);

                    if (!(currentBodyTransition == BodyTransitionIndex.NULL))
                        bodyTransitions.draw(spriteBatch, spriteFlipping);
                    else
                        body.draw(spriteBatch, spriteFlipping);
                }
                else
                {
                    legs.draw(spriteBatch, spriteFlipping);
                    body.draw(spriteBatch, spriteFlipping);
                }
            }
            else
            {

                if (!(currentBodyTransition == BodyTransitionIndex.NULL))
                {
                    if (!(currentLegTransition == LegTransitionIndex.NULL))
                        legsTransitions.draw(spriteBatch, spriteFlipping);
                    else
                        legs.draw(spriteBatch, spriteFlipping);

                    bodyTransitions.draw(spriteBatch, spriteFlipping);
                }
                else if (!(currentLegTransition == LegTransitionIndex.NULL))
                {
                    legsTransitions.draw(spriteBatch, spriteFlipping);

                    if (!(currentBodyTransition == BodyTransitionIndex.NULL))
                        bodyTransitions.draw(spriteBatch, spriteFlipping);
                    else
                        body.draw(spriteBatch, spriteFlipping);
                }
                else
                {
                    legs.draw(spriteBatch, spriteFlipping);
                    body.draw(spriteBatch, spriteFlipping);
                }
            }
        }

        private void move(KeyboardState keyState, KeyboardState oldKeyState, Rectangle viewportRect, double elapsedTime, GameTime gameTime)
        {

            if (keyState.IsKeyDown(Keys.Space) && !oldKeyState.IsKeyDown(Keys.Space) && !crouching)
            {
                if (jumpsLeft <= 2 && jumpsLeft > 0)
                {
                    heroState = HeroStates.Jumping;
                    body.setCurrentActiveSprite((int)BodySpriteIndex.Jump);
                    legs.setCurrentActiveSprite((int)LegSpriteIndex.Jump);
                    setCheckStrings("jumping");
                    jump();

                }
            }

            if (heroState == HeroStates.StandStill && !oldKeyState.IsKeyDown(Keys.Down) && keyState.IsKeyDown(Keys.Down)) // case for idle -> crouch transition
                drawCrouchingTransition(keyState, oldKeyState);
            if ((heroState == HeroStates.CrouchStill || heroState == HeroStates.CrouchWalking) && oldKeyState.IsKeyDown(Keys.Down) && keyState.IsKeyUp(Keys.Down)) // case for crouch/crouchwalking -> uncrouch transition
                drawUnCrouchingTransition(keyState, oldKeyState);

            if (keyState.IsKeyDown(Keys.Down) && (!keyState.IsKeyDown(Keys.Right) || !keyState.IsKeyDown(Keys.Left)) && onGround) // case for crouch //
            {
                crouching = true;
                heroState = HeroStates.CrouchStill;
                body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                 
                setCheckStrings("crouchidle");
            }
            
            if (keyState.IsKeyDown(Keys.Down) && (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.Left)) && onGround) // case for moving into crouch  //
            {
                if (keyState.IsKeyDown(Keys.Left))
                {
                    checkFacingRight(keyState, oldKeyState);
                    spriteFlipping = true;
                    setLegsOffsetTrue();
                }
                else
                {
                    checkFacingLeft(keyState, oldKeyState);
                    spriteFlipping = false;
                    setLegsOffsetFalse();
                }

                if (!oldKeyState.IsKeyDown(Keys.Down) && keyState.IsKeyDown(Keys.Down))
                {
                    drawCrouchingTransition(keyState, oldKeyState);    
                }
                else if( keyState.IsKeyDown(Keys.Down) && oldKeyState.IsKeyDown(Keys.Up) && !keyState.IsKeyDown(Keys.Up))
                {
                    drawCrouchingTransition(keyState, oldKeyState); // CASE FOR LOOKING UP, THEN PRESS CROUCH, THEN LET GO OF LOOKING UP // 
                }
                else
                {
                    crouching = true;
                    heroState = HeroStates.CrouchWalking;
                    body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                    legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                }
            }

            if ( keyState.IsKeyUp(Keys.Down) &&  (!keyState.IsKeyDown(Keys.Right) || !keyState.IsKeyDown(Keys.Left)) && onGround) // case for standing still //
            {
                crouching = false;
                heroState = HeroStates.StandStill;
                body.setCurrentActiveSprite((int)BodySpriteIndex.Idle);
                legs.setCurrentActiveSprite((int)LegSpriteIndex.Idle);
                setCheckStrings("idle");
                checkMidToUpConditions(keyState, oldKeyState);
                checkUpToMidConditions(keyState, oldKeyState);
            }

            if (keyState.IsKeyDown(Keys.Right)) // case for walking right & crouch walking & pressing left and right //
            {
                if (keyState.IsKeyDown(Keys.Down))
                {
                    spriteFlipping = false;
                    checkFacingLeft(keyState, oldKeyState);
                    setLegsOffsetFalse();
                    crouching = true;
                    heroState = HeroStates.CrouchWalking;
                    moveRight(gameTime);
                    body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                    
                    if(!keyState.IsKeyDown(Keys.Left))
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchWalk);
                    else
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                }
                else if(keyState.IsKeyDown(Keys.Left))
                {
                    crouching = false;
                    heroState = HeroStates.StandStill;
                    body.setCurrentActiveSprite((int)BodySpriteIndex.Idle);

                    if (!keyState.IsKeyDown(Keys.Down))
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.Idle);
                    else
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                    checkMidToUpConditions(keyState, oldKeyState);
                    checkUpToMidConditions(keyState, oldKeyState);
                }
                else
                {
                    spriteFlipping = false;
                    checkFacingLeft(keyState, oldKeyState);
                    setLegsOffsetFalse();
                    heroState = HeroStates.Running;
                    moveRight(gameTime);
                    body.setCurrentActiveSprite((int)BodySpriteIndex.Idle);
                    legs.setCurrentActiveSprite((int)LegSpriteIndex.Running);                  
                    checkMidToUpConditions(keyState, oldKeyState);
                    checkUpToMidConditions(keyState, oldKeyState);
                }
            }
            if (keyState.IsKeyDown(Keys.Left)) // case for walking left and crouching //
            {
                if (keyState.IsKeyDown(Keys.Down))
                {
                    spriteFlipping = true;
                    checkFacingRight(keyState, oldKeyState);
                    setLegsOffsetTrue();
                    heroState = HeroStates.CrouchWalking;
                    moveLeft(gameTime);
                    body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);

                    if(!keyState.IsKeyDown(Keys.Right))
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchWalk);
                    else
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                }
                else if (keyState.IsKeyDown(Keys.Right))
                {
                    crouching = false;
                    heroState = HeroStates.StandStill;
                    body.setCurrentActiveSprite((int)BodySpriteIndex.Idle);

                    if(!keyState.IsKeyDown(Keys.Down))
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.Idle);
                    else
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);

                    checkMidToUpConditions(keyState, oldKeyState);
                    checkUpToMidConditions(keyState, oldKeyState);
                }
                else
                {
                    spriteFlipping = true;
                    checkFacingRight(keyState, oldKeyState);
                    setLegsOffsetTrue();
                    heroState = HeroStates.Running;
                  
                    moveLeft(gameTime);
                    body.setCurrentActiveSprite((int)BodySpriteIndex.Idle);
                    legs.setCurrentActiveSprite((int)LegSpriteIndex.Running);
                     

                    checkMidToUpConditions(keyState, oldKeyState);
                    checkUpToMidConditions(keyState, oldKeyState);
                }
            }
            
        }


        private void handleFalling(GameTime gameTime)
        {
            frameLimiter += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (frameLimiter >= MILLISECOND_DELAY)
            {
                frameLimiter = 0;

                if (jumpAcceleration <= 0 && (floor.intersects(standingHitbox) || obstacle.intersects(standingHitbox)) )
                {
                    falling = false;
                    airborn = false;
                    doubleJumping = false;
                    jumpAcceleration = 0;
                    jumpsLeft = 2;
                }
                else
                {
                    if (!falling && jumpAcceleration < 0) // highest point of jump
                        falling = true;
                    if (!airborn)
                        airborn = true;
                    if (jumpAcceleration < -40)
                        jumpAcceleration = -40;
                    else if (jumpAcceleration > -40)
                        jumpAcceleration -= 3;

                    int absJumpAccel = Math.Abs(jumpAcceleration);

                    for (int counter = 0; counter < absJumpAccel; counter++)
                    {
                        if (jumpAcceleration <= 0 && ((floor.intersects(standingHitbox)) || obstacle.intersects(standingHitbox)) )
                            break;
                        if (jumpAcceleration > 0)
                        {
                            body.position.Y--;
                            legs.position.Y--;
                            legsTransitions.position.Y--;
                            bodyTransitions.position.Y--;
                            updateHitboxes();
                        }
                        else
                        {
                            body.position.Y++;
                            legs.position.Y++;
                            legsTransitions.position.Y++;
                            bodyTransitions.position.Y++;
                            updateHitboxes();
                        }
                    }

                }
            }
        }

        private void setLegsOffsetFalse()
        {
            if (offsetCheck)
            {
                legs.subtXOffset(2);
                body.subtXOffset(-55);
                legsTransitions.subtXOffset(2);
                bodyTransitions.subtXOffset(-55);
                offsetCheck = false;
            }
        }
        private void setLegsOffsetTrue()
        {
            if (!offsetCheck)
            {
                legs.addXOffset(2);
                body.addXOffset(-55);
                legsTransitions.addXOffset(2);
                bodyTransitions.addXOffset(-55);
                offsetCheck = true;
            }
        }

        private void setCheckStrings(string newString)
        {
            if (!bodyString.Equals(newString))
            {
                bodyString = newString;
                body.resetAnimationValues();
            }
            if (!legString.Equals(newString))
            {
                legString = newString;
                legs.resetAnimationValues();
            }
        }
        private void setTransitionCheckStrings(string newString)
        {
            if (!bodyString.Equals(newString))
            {
                bodyString = newString;
                bodyTransitions.resetAnimationValues();
            }
            if (!legString.Equals(newString))
            {
                legString = newString;
                legsTransitions.resetAnimationValues();
            }
        }

        
        private void jump()
        {
            jumpAcceleration = 32;
            jumpsLeft--;
        }

        public bool intersects(Rectangle rectangle)
        {
            if (isStandingHitbox)
                return standingHitbox.Intersects(rectangle);
            else
                return crouchingHitbox.Intersects(rectangle);
        }
        public bool intersects(Colideable obj)
        {
            if (isStandingHitbox)
                return standingHitbox.Intersects(obj.getRect());
            else
                return crouchingHitbox.Intersects(obj.getRect());
        }
        public Rectangle getRect()
        {
            if (isStandingHitbox)
                return standingHitbox;
            else
                return crouchingHitbox;
        }
        
        private void moveRight(GameTime gameTime) 
        {
            if (!crouching)
            {
                if (!obstacle.intersects(standingHitbox))
                {
                    body.move(movespeed);
                    legs.move(movespeed);
                    bodyTransitions.move(movespeed);
                    legsTransitions.move(movespeed);
                }
                else if (obstacle.getRect().Top + 20 > standingHitbox.Bottom)
                {
                    body.move(movespeed);
                    legs.move(movespeed);
                    bodyTransitions.move(movespeed);
                    legsTransitions.move(movespeed);
                }
            }
            else
            {
                if (!obstacle.intersects(standingHitbox))
                {
                    body.move(movespeed / 2);
                    legs.move(movespeed / 2);
                    bodyTransitions.move(movespeed / 2);
                    legsTransitions.move(movespeed / 2);
                }
                else if (obstacle.getRect().Top + 20 > standingHitbox.Bottom)
                {
                    body.move(movespeed);
                    legs.move(movespeed);
                    bodyTransitions.move(movespeed);
                    legsTransitions.move(movespeed);
                }
            }
            facingRight = true;
        }
        private void moveLeft(GameTime gameTime) 
        {
            if (!crouching)
            {
                if (!obstacle.intersects(standingHitbox))
                {
                    body.move(-movespeed);
                    legs.move(-movespeed);
                    bodyTransitions.move(-movespeed);
                    legsTransitions.move(-movespeed);
                }
                else if(obstacle.getRect().Top + 20 > standingHitbox.Bottom)
                {
                    body.move(-movespeed);
                    legs.move(-movespeed);
                    bodyTransitions.move(-movespeed);
                    legsTransitions.move(-movespeed);
                }
            }
            else
            {
                if (!obstacle.intersects(standingHitbox))
                {
                    body.move(-movespeed / 2);
                    legs.move(-movespeed / 2);
                    bodyTransitions.move(-movespeed / 2);
                    legsTransitions.move(-movespeed / 2);
                }
                else if (obstacle.getRect().Top + 20 > standingHitbox.Bottom)
                {
                    body.move(-movespeed);
                    legs.move(-movespeed);
                    bodyTransitions.move(-movespeed);
                    legsTransitions.move(-movespeed);
                }
            }
            facingRight = false;
        }

        public void updateHitboxes()
        {
            if (!crouching && facingRight)
            {
                standingHitbox.X = (int)body.position.X - 70;
                standingHitbox.Y = (int)body.position.Y - 43;
            }
            else if (crouching && facingRight)
            {
                standingHitbox.X = (int)body.position.X - 70;
                standingHitbox.Y = (int)body.position.Y - 43;
            }
            else if (!crouching && !facingRight)
            {
                standingHitbox.X = (int)body.position.X - 25;
                standingHitbox.Y = (int)body.position.Y - 43;
            }
            else if (crouching && !facingRight)
            {
                standingHitbox.X = (int)body.position.X - 25;
                standingHitbox.Y = (int)body.position.Y - 43;
            }
        }

        static public void setBodyNull()
        {
            currentBodyTransition = BodyTransitionIndex.NULL;
        }
        static public void setLegNull()
        {
            currentLegTransition = LegTransitionIndex.NULL;
        }

        private void drawCrouchingTransition(KeyboardState keyState, KeyboardState oldKeySate)
        {
                // this is the condition for a transition to be played. //
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                
                currentLegTransition = LegTransitionIndex.Crouch;
                legsTransitions.setCurrentActiveSprite((int)LegTransitionIndex.Crouch);
               
                if (!keyState.IsKeyDown(Keys.Up))
                {
                    currentBodyTransition = BodyTransitionIndex.Crouch;
                    bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.Crouch);
                }
                setTransitionCheckStrings("crouchtransition");
        }
        private void drawUnCrouchingTransition(KeyboardState keyState, KeyboardState oldKeySate)
        {
            legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
            bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

            currentLegTransition = LegTransitionIndex.UnCrouch;
            legsTransitions.setCurrentActiveSprite((int)LegTransitionIndex.UnCrouch);
            
            if (!keyState.IsKeyDown(Keys.Up))
            {
                currentBodyTransition = BodyTransitionIndex.UnCrouch;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.UnCrouch);
            }
            setTransitionCheckStrings("uncrouchtransition");
        }
        
        private void drawMidToUpTransition(KeyboardState keyState, KeyboardState oldKeyState)
        {
            bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;
            legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

            currentBodyTransition = BodyTransitionIndex.MidToUp;
            bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MidToUp);
            setTransitionCheckStrings("midtouptransition");

        }
        private void checkMidToUpConditions(KeyboardState keyState, KeyboardState oldKeyState)
        {
            if (keyState.IsKeyDown(Keys.Up))
            {
                facingUp = true;

                if (!oldKeyState.IsKeyDown(Keys.Up))
                    drawMidToUpTransition(keyState, oldKeyState);
                else
                    body.setCurrentActiveSprite((int)BodySpriteIndex.IdleUp);
            }
        }

        private void drawUpToMidTransition(KeyboardState keyState, KeyboardState oldKeyState)
        {
            bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;
            legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

            currentBodyTransition = BodyTransitionIndex.UpToMid;
            bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.UpToMid);
            setTransitionCheckStrings("uptomidtransition");
        }
        private void checkUpToMidConditions(KeyboardState keyState, KeyboardState oldKeyState)
        {
            if (keyState.IsKeyUp(Keys.Up))
            {
                if (!oldKeyState.IsKeyUp(Keys.Up))
                {
                    drawUpToMidTransition(keyState, oldKeyState);
                    facingUp = false;
                }
            }
        }

        private void drawTurningTransition(KeyboardState keyState, KeyboardState oldKeyState)
        {
            bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;
            legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

            if (!crouching && !facingUp)
            {
                currentBodyTransition = BodyTransitionIndex.Turn;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.Turn);
                setTransitionCheckStrings("turntransition");

            }
            else if(crouching && !facingUp)
            {
                currentBodyTransition = BodyTransitionIndex.CrouchTurn;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.CrouchTurn);
                setTransitionCheckStrings("crouchturntransition");
            }
            else if (facingUp && !crouching)
            {
                currentBodyTransition = BodyTransitionIndex.TurnUp;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.TurnUp);
                setTransitionCheckStrings("turnuptransition");
            }

        }
        private void checkFacingLeft(KeyboardState keyState, KeyboardState oldKeyState)
        {
            if (!facingRight)
            {
                facingRight = true;
                drawTurningTransition(keyState, oldKeyState);
            }
        }
        private void checkFacingRight(KeyboardState keyState, KeyboardState oldKeyState)
        {
            if (facingRight)
            {
                facingRight = false;
                drawTurningTransition(keyState, oldKeyState);
            }
        }

        private void setBodyHardCodedVals() 
        {
            amountOfSheets = 19;

            imgPaths[(int)BodySpriteIndex.CrouchIdle] = "Sprites//Hero//MACHINE GUN//machine_gun_crouchidle";
            imgPaths[(int)BodySpriteIndex.CrouchMagic] = "Sprites//Hero//MACHINE GUN//machine_gun_crouchmagic";
            imgPaths[(int)BodySpriteIndex.CrouchReload] = "Sprites//Hero//MACHINE GUN//machine_gun_crouchreload";
            imgPaths[(int)BodySpriteIndex.CrouchShoot] = "Sprites//Hero//MACHINE GUN//machine_gun_crouchshoot";
            imgPaths[(int)BodySpriteIndex.Fall] = "Sprites//Hero//MACHINE GUN//machine_gun_fall";
            imgPaths[(int)BodySpriteIndex.GetHurt] = "Sprites//Hero//MACHINE GUN//machine_gun_gethurt";
            imgPaths[(int)BodySpriteIndex.HangIdle] = "Sprites//Hero//MACHINE GUN//machine_gun_hangidle";
            imgPaths[(int)BodySpriteIndex.Idle] = "Sprites//Hero//MACHINE GUN//machine_gun_idle";
            imgPaths[(int)BodySpriteIndex.IdleUp] = "Sprites//Hero//MACHINE GUN//machine_gun_idleup";
            imgPaths[(int)BodySpriteIndex.Jump] = "Sprites//Hero//MACHINE GUN//machine_gun_jump";
            imgPaths[(int)BodySpriteIndex.Magic] = "Sprites//Hero//MACHINE GUN//machine_gun_magic";
            imgPaths[(int)BodySpriteIndex.MagicDown] = "Sprites//Hero//MACHINE GUN//machine_gun_magicdown";
            imgPaths[(int)BodySpriteIndex.MagicUp] = "Sprites//Hero//MACHINE GUN//machine_gun_magicup";
            imgPaths[(int)BodySpriteIndex.PullTowards] = "Sprites//Hero//MACHINE GUN//machine_gun_pulltowards";
            imgPaths[(int)BodySpriteIndex.PushAway] = "Sprites//Hero//MACHINE GUN//machine_gun_pushaway";
            imgPaths[(int)BodySpriteIndex.ReloadUp] = "Sprites//Hero//MACHINE GUN//machine_gun_reloadup";
            imgPaths[(int)BodySpriteIndex.Shoot] = "Sprites//Hero//MACHINE GUN//machine_gun_shoot";
            imgPaths[(int)BodySpriteIndex.ShootDown] = "Sprites//Hero//MACHINE GUN//machine_gun_shootdown";
            imgPaths[(int)BodySpriteIndex.ShootUp] = "Sprites//Hero//MACHINE GUN//machine_gun_shootup";

            frameAmounts[(int)BodySpriteIndex.CrouchIdle] = 25;
            frameAmounts[(int)BodySpriteIndex.CrouchMagic] = 5;
            frameAmounts[(int)BodySpriteIndex.CrouchReload] = 19;
            frameAmounts[(int)BodySpriteIndex.CrouchShoot] = 4;
            frameAmounts[(int)BodySpriteIndex.Fall] = 7;
            frameAmounts[(int)BodySpriteIndex.GetHurt] = 6;
            frameAmounts[(int)BodySpriteIndex.HangIdle] = 25;
            frameAmounts[(int)BodySpriteIndex.Idle] = 25;
            frameAmounts[(int)BodySpriteIndex.IdleUp] = 25;
            frameAmounts[(int)BodySpriteIndex.Jump] = 7;
            frameAmounts[(int)BodySpriteIndex.Magic] = 5;
            frameAmounts[(int)BodySpriteIndex.MagicDown] = 5;
            frameAmounts[(int)BodySpriteIndex.MagicUp] = 5;
            frameAmounts[(int)BodySpriteIndex.PullTowards] = 6;
            frameAmounts[(int)BodySpriteIndex.PushAway] = 6;
            frameAmounts[(int)BodySpriteIndex.ReloadUp] = 19;
            frameAmounts[(int)BodySpriteIndex.Shoot] = 4;
            frameAmounts[(int)BodySpriteIndex.ShootDown] = 4;
            frameAmounts[(int)BodySpriteIndex.ShootUp] = 4;

            for (int counter = 0; counter < amountOfSheets; counter++)
            {
                frameRects[counter] = new Rectangle(0,0,186,208);
                spriteDelayTimes[counter] = MILLISECOND_DELAY;
            }

            startingPos = new Vector2(bodyStartPos.X, bodyStartPos.Y);
            
        }
        private void setLegHardCodedVals() 
        {
            amountOfSheets = 7; // this is for the TOTAL amount of sheets that wil be loaded (this number will obviously increase over time as different animations will be needed) //

            imgPaths[(int)LegSpriteIndex.CrouchWalk] = "Sprites//Hero//LEGS//crouchwalk";
            imgPaths[(int)LegSpriteIndex.Fall] = "Sprites//Hero//LEGS//fall";
            imgPaths[(int)LegSpriteIndex.GetHurt] = "Sprites//Hero//LEGS//gethurt";
            imgPaths[(int)LegSpriteIndex.Idle] = "Sprites//Hero//LEGS//idle";
            imgPaths[(int)LegSpriteIndex.Jump] = "Sprites//Hero//LEGS//jump";
            imgPaths[(int)LegSpriteIndex.Running] = "Sprites//Hero//LEGS//running";
            imgPaths[(int)LegSpriteIndex.CrouchIdle] = "Sprites//Hero//LEGS//crouchwalk";
            

            frameAmounts[(int)LegSpriteIndex.CrouchWalk] = 14;
            frameAmounts[(int)LegSpriteIndex.Fall] = 2;
            frameAmounts[(int)LegSpriteIndex.GetHurt] = 6;
            frameAmounts[(int)LegSpriteIndex.Idle] = 25;
            frameAmounts[(int)LegSpriteIndex.Jump] = 9;
            frameAmounts[(int)LegSpriteIndex.Running] = 15;
            frameAmounts[(int)LegSpriteIndex.CrouchIdle] = 1;

            for (int counter = 0; counter < amountOfSheets; counter++)
            {
                frameRects[counter] = new Rectangle(0, 0, 111, 91);
                spriteDelayTimes[counter] = MILLISECOND_DELAY;
            }

            /*startingPos[(int)LegSpriteIndex.CrouchWalk] = new Vector2(legsStartPos.X, legsStartPos.Y);
            startingPos[(int)LegSpriteIndex.Running] = new Vector2(legsStartPos.X, legsStartPos.Y);
            startingPos[(int)LegSpriteIndex.Idle] = new Vector2(legsStartPos.X, legsStartPos.Y);
            */

            startingPos = new Vector2(legsStartPos.X, legsStartPos.Y);
          
        }
        private void setSingleSheetHardCodedVals()
        {

            // this is ALL incorrect, needs to change to the correct single sheets //

            amountOfSheets = 3; // this is for the TOTAL amount of sheets that wil be loaded (this number will obviously increase over time as different animations will be needed) //

            imgPaths[(int)LegSpriteIndex.CrouchWalk] = "Sprites//Hero//LEGS//crouchwalk";
            imgPaths[(int)LegSpriteIndex.Running] = "Sprites//Hero//LEGS//running";
            imgPaths[(int)LegSpriteIndex.Idle] = "Sprites//Hero//LEGS//idle";

            frameAmounts[(int)LegSpriteIndex.CrouchWalk] = 1;
            frameAmounts[(int)LegSpriteIndex.Running] = 24;
            frameAmounts[(int)LegSpriteIndex.Idle] = 25;

            for (int counter = 0; counter < amountOfSheets; counter++)
            {
                frameRects[counter] = new Rectangle(0, 0, 101, 86);
                spriteDelayTimes[counter] = MILLISECOND_DELAY;
            }

            /*startingPos[(int)LegSpriteIndex.CrouchWalk] = new Vector2(legsStartPos.X, legsStartPos.Y);
            startingPos[(int)LegSpriteIndex.Running] = new Vector2(legsStartPos.X, legsStartPos.Y);
            startingPos[(int)LegSpriteIndex.Idle] = new Vector2(legsStartPos.X, legsStartPos.Y);
             * */
            startingPos = new Vector2(legsStartPos.X, legsStartPos.Y);
        }
        private void setBodyTransitionHardCodedValues()
        {
            amountOfSheets = 15; // this is for the TOTAL amount of sheets that wil be loaded (this number will obviously increase over time as different animations will be needed) //

            imgPaths[(int)BodyTransitionIndex.Crouch] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_crouch";
            imgPaths[(int)BodyTransitionIndex.CrouchHolster] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_crouchholster"; // new
            imgPaths[(int)BodyTransitionIndex.CrouchTurn] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_crouchturn"; 
            //imgPaths[(int)BodyTransitionIndex.CrouchUnHolster] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_crouchunholster";// new
            imgPaths[(int)BodyTransitionIndex.Hoist] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_hoist";
            imgPaths[(int)BodyTransitionIndex.Holster] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_holster";
            imgPaths[(int)BodyTransitionIndex.LowToMid] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_lowtomid";
            imgPaths[(int)BodyTransitionIndex.MidToLow] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_midtolow";
            imgPaths[(int)BodyTransitionIndex.MidToUp] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_midtoup";
            imgPaths[(int)BodyTransitionIndex.Turn] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_turn";
            imgPaths[(int)BodyTransitionIndex.TurnUp] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_turnup";
            imgPaths[(int)BodyTransitionIndex.TurnDown] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_turndown"; // new
            imgPaths[(int)BodyTransitionIndex.UnCrouch] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_uncrouch";
            //imgPaths[(int)BodyTransitionIndex.UnHolster] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_unholster";
            imgPaths[(int)BodyTransitionIndex.UpToMid] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_uptomid";


            frameAmounts[(int)BodyTransitionIndex.Crouch] = 6;
            frameAmounts[(int)BodyTransitionIndex.CrouchTurn] = 4;
            frameAmounts[(int)BodyTransitionIndex.CrouchHolster] = 8; 
            frameAmounts[(int)BodyTransitionIndex.TurnDown] = 4; 
            frameAmounts[(int)BodyTransitionIndex.Hoist] = 14;
            frameAmounts[(int)BodyTransitionIndex.Holster] = 8;
            frameAmounts[(int)BodyTransitionIndex.LowToMid] = 4;
            frameAmounts[(int)BodyTransitionIndex.MidToLow] = 4;
            frameAmounts[(int)BodyTransitionIndex.MidToUp]  = 4;
            frameAmounts[(int)BodyTransitionIndex.Turn] = 4;
            frameAmounts[(int)BodyTransitionIndex.TurnUp] = 4;
            //frameAmounts[(int)BodyTransitionIndex.CrouchUnHolster] = 4; 
            frameAmounts[(int)BodyTransitionIndex.UnCrouch] = 6;
            //frameAmounts[(int)BodyTransitionIndex.UnHolster] = 4;
            frameAmounts[(int)BodyTransitionIndex.UpToMid] = 4;

            for (int counter = 0; counter < amountOfSheets; counter++)
            {
                frameRects[counter] = new Rectangle(0, 0, 186, 208);
                spriteDelayTimes[counter] = MILLISECOND_DELAY;
               // startingPos[counter] = new Vector2(bodyStartPos.X, bodyStartPos.Y);
            }
            startingPos = new Vector2(bodyStartPos.X, bodyStartPos.Y);
        }
        private void setLegsTransitionHardCodedValues()
        {
            amountOfSheets = 7; // total number of sheets to be loaded //

            imgPaths[(int)LegTransitionIndex.Crouch] = "Sprites//Hero//LEGS//Transitions//crouch";
            imgPaths[(int)LegTransitionIndex.UnCrouch] = "Sprites//Hero//LEGS//Transitions//uncrouch";

            frameAmounts[(int)LegTransitionIndex.Crouch]  = 5;
            frameAmounts[(int)LegTransitionIndex.UnCrouch] = 7;
            
            
            for (int counter = 0; counter < amountOfSheets; counter++)
            {
                frameRects[counter] = new Rectangle(0, 0, 111, 91);
                spriteDelayTimes[counter] = MILLISECOND_DELAY;
                //startingPos[counter] = new Vector2(legsStartPos.X, legsStartPos.Y);
            }
            startingPos = new Vector2(legsStartPos.X, legsStartPos.Y);
        }
        private void initalizeStagingArrays()
        {
            frameAmounts = new int[25]; 
            frameRects = new Rectangle[25];
            imgPaths = new string[25];
            //startingPos = new Vector2[20];
            spriteDelayTimes = new int[25];
        }

        public void passPlatform(Platform plat, Platform obs)
        {
            floor = plat;
            obstacle = obs;
        } // for debugging collision & jumping //
    }
}
