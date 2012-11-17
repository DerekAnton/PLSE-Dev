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

    public enum BodyTransitionIndex { Crouch, CrouchHolster, CrouchTurn, /*new*/GrabLedge, Hoist, Holster, /*new*/HolsterDown, /*new*/HolsterUp, /*new*/LetGo, LowToMid,
        /*new*/MagicCrouchEnd, /*new*/MagicCrouchStart,/*new*/ MagicDownEnd, /*new*/MagicDownStart, /*new*/MagicEnd, /*new*/MagicStart, /*new*/MagicUpEnd, /*new*/MagicUpStart, MidToLow, MidToUp, 
        /*new*/ShootCrouchEnd, /*new*/ShootCrouchStart, /*new*/ShootDownEnd, /*new*/ShootDownStart, /*new*/ShootEnd, /*new*/ShootStart, /*new*/ShootUpEnd, /*new*/ShootUpStart, Turn, TurnDown, 
        TurnUp, UnCrouch, UpToMid, NULL };
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

        private bool firing = false;
        private bool firingmagic = false;

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

        private bool movementlock = false; // used specificly to choose a direction when the player hits down, right, and left all at the same time (so we dont end up adding and subtracting a movement in the air, and effectivley moving nowhere)

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
            movementlock = false;
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
            // Jumping Logic // 
            if (keyState.IsKeyDown(Keys.Space) && !oldKeyState.IsKeyDown(Keys.Space))
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

            // Idle To Crouch Transition Cases //            
            if (heroState == HeroStates.StandStill && !oldKeyState.IsKeyDown(Keys.Down) && keyState.IsKeyDown(Keys.Down) && !airborn) // case for idle -> crouch transition
                drawCrouchingTransition(keyState, oldKeyState);
            if ((heroState == HeroStates.CrouchStill || heroState == HeroStates.CrouchWalking) && oldKeyState.IsKeyDown(Keys.Down) && keyState.IsKeyUp(Keys.Down) && !airborn) // case for crouch/crouchwalking -> uncrouch transition
                drawUnCrouchingTransition(keyState, oldKeyState);
          
            // Crouch Idle //
            if (keyState.IsKeyDown(Keys.Down) && (!keyState.IsKeyDown(Keys.Right) || !keyState.IsKeyDown(Keys.Left)) && !airborn)
            {
                crouching = true;
                heroState = HeroStates.CrouchStill;
                body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                 
                setCheckStrings("crouchidle");
            }
            
            // Hero Idle //
            if ( keyState.IsKeyUp(Keys.Down) &&  (!keyState.IsKeyDown(Keys.Right) || !keyState.IsKeyDown(Keys.Left))) 
            {
                crouching = false;
                heroState = HeroStates.StandStill;
                body.setCurrentActiveSprite((int)BodySpriteIndex.Idle);
                legs.setCurrentActiveSprite((int)LegSpriteIndex.Idle);
                setCheckStrings("idle");
                checkMidToUpConditions(keyState, oldKeyState);
                checkUpToMidConditions(keyState, oldKeyState);
            }

            // Hero Walking Right Conditions (Right Walking, Right Crouch Walking, Etc...) //
            if (keyState.IsKeyDown(Keys.Right))
            {
                if (keyState.IsKeyDown(Keys.Down)) // Right Down //
                {
                    spriteFlipping = false;
                    crouching = true;
                    checkFacingLeft(keyState, oldKeyState);
                    setLegsOffsetFalse();

                    // Crouching Transitions //
                    if (!oldKeyState.IsKeyDown(Keys.Down) && keyState.IsKeyDown(Keys.Down) && !airborn)
                        drawCrouchingTransition(keyState, oldKeyState);
                    else if (keyState.IsKeyDown(Keys.Down) && oldKeyState.IsKeyDown(Keys.Up) && !keyState.IsKeyDown(Keys.Up) && !airborn)
                        drawCrouchingTransition(keyState, oldKeyState);
                    
                    // Crouch Walking Right Condition //
                    if (keyState.IsKeyDown(Keys.Left) && !airborn)                                       // condition for right, down, left & not airborn //
                    {
                        heroState = HeroStates.CrouchStill;
                        body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                    }
                    else if (!keyState.IsKeyDown(Keys.Left) && !airborn)                                 // condition for crouched movement (right) on ground//
                    {
                        body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchWalk);
                        moveRight(gameTime);
                    }
                    else if (keyState.IsKeyDown(Keys.Left) && airborn)                                   // right down left and airborn //
                    {
                        crouching = false;
                        body.setCurrentActiveSprite((int)BodySpriteIndex.ShootDown);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.Jump);
                        if (!movementlock)
                        {
                            moveRight(gameTime);
                            movementlock = true;
                        }
                    }
                    else if (!keyState.IsKeyDown(Keys.Left) && airborn)                                  // right and down and airborn //
                    {
                        crouching = false;
                        body.setCurrentActiveSprite((int)BodySpriteIndex.ShootDown);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.Jump);
                        if (!movementlock)
                        {
                            moveRight(gameTime);
                            movementlock = true;
                        }
                    } 
                }
                else if(keyState.IsKeyDown(Keys.Left)) // Right Left //
                {
                    spriteFlipping = false;
                    crouching = false;
                    checkFacingLeft(keyState, oldKeyState);
                    setLegsOffsetFalse();

                    if (!keyState.IsKeyDown(Keys.Down) && !airborn)                                     // case for right left & not airborn//
                    {
                        crouching = false;
                        body.setCurrentActiveSprite((int)BodySpriteIndex.Idle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.Idle);
                    }
                    else if (keyState.IsKeyDown(Keys.Down) && !airborn)                                 // right left down (not in the air) [Results in crouch idle] // 
                    {
                        crouching = true;
                        heroState = HeroStates.CrouchStill;

                        // Crouching Transitions //
                        if (!oldKeyState.IsKeyDown(Keys.Down) && keyState.IsKeyDown(Keys.Down) && !airborn)
                            drawCrouchingTransition(keyState, oldKeyState);
                        else if (keyState.IsKeyDown(Keys.Down) && oldKeyState.IsKeyDown(Keys.Up) && !keyState.IsKeyDown(Keys.Up) && !airborn)
                            drawCrouchingTransition(keyState, oldKeyState);
                    
                        body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                    }
                    else if (keyState.IsKeyDown(Keys.Down) && airborn)                                   // right left down & airborn// 
                    {
                        crouching = false;
                        body.setCurrentActiveSprite((int)BodySpriteIndex.ShootDown);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.Jump);
                        if (!movementlock)
                        {
                            moveRight(gameTime);
                            movementlock = true;
                        }
                    }
                    checkMidToUpConditions(keyState, oldKeyState);
                    checkUpToMidConditions(keyState, oldKeyState);
                }
                else                                                                                     // case for running right //
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
            if (keyState.IsKeyDown(Keys.Left)) // Left Key Check
            {
                if (keyState.IsKeyDown(Keys.Down))                                                       //  Left & Down //
                {
                    spriteFlipping = true;
                    checkFacingRight(keyState, oldKeyState);
                    setLegsOffsetTrue();
                    
                    // Crouching Transition //
                    if (!oldKeyState.IsKeyDown(Keys.Down) && keyState.IsKeyDown(Keys.Down) && !airborn)
                        drawCrouchingTransition(keyState, oldKeyState);
                    else if (keyState.IsKeyDown(Keys.Down) && oldKeyState.IsKeyDown(Keys.Up) && !keyState.IsKeyDown(Keys.Up) && !airborn)
                        drawCrouchingTransition(keyState, oldKeyState); // CASE FOR LOOKING UP, THEN PRESS CROUCH, THEN LET GO OF LOOKING UP // 

                    if (!keyState.IsKeyDown(Keys.Right) && !airborn)                                    // Left Down And Not Airborn //
                    {
                        crouching = true;
                        heroState = HeroStates.CrouchWalking;
                        body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchWalk);
                        moveLeft(gameTime);
                    }
                    if(keyState.IsKeyDown(Keys.Right) && !airborn)                                 // Left Down Right and Not Airborn// 
                    {
                        crouching = true;
                        heroState = HeroStates.CrouchStill;
                        body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);

                    }
                    if (!keyState.IsKeyDown(Keys.Right) && airborn)                                // Left Down Jumping //
                    {
                        crouching = false;
                        body.setCurrentActiveSprite((int)BodySpriteIndex.ShootDown);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.Jump);
                        if (!movementlock)
                        {
                            moveLeft(gameTime);
                            movementlock = true;
                        }
                    }
                    if (keyState.IsKeyDown(Keys.Right) && airborn)                                 // Left Down Right And Airborn //
                    {
                        crouching = false;
                        body.setCurrentActiveSprite((int)BodySpriteIndex.ShootDown);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.Jump);
                        if (!movementlock)
                        {
                            moveLeft(gameTime);
                            movementlock = true;
                        }
                    }
                    checkMidToUpConditions(keyState, oldKeyState);
                    checkUpToMidConditions(keyState, oldKeyState);
                }
                else if (keyState.IsKeyDown(Keys.Right))                                                // Left & Right //
                {
                    if (!keyState.IsKeyDown(Keys.Down) && !airborn)                                     // Right Left  Only //
                    {
                        crouching = false;
                        heroState = HeroStates.StandStill;
                        body.setCurrentActiveSprite((int)BodySpriteIndex.Idle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.Idle);
                    }
                    else if (keyState.IsKeyDown(Keys.Down) && !airborn)                                 // Right Left Down Grounded //
                    {
                        crouching = true;
                        heroState = HeroStates.CrouchStill;
                        body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                    }
                    else if (keyState.IsKeyDown(Keys.Down) && airborn)                                  // Left Right Down airborn //
                    {
                        crouching = true;
                        heroState = HeroStates.CrouchStill;
                        body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                        if (!movementlock)
                        {
                            moveRight(gameTime);
                            movementlock = true;
                        }
                    }
                    checkMidToUpConditions(keyState, oldKeyState);
                    checkUpToMidConditions(keyState, oldKeyState);
                }
                else                                                                                    // Running //
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
           
            //Shooting Animation Logic //
            if (keyState.IsKeyDown(Keys.D) && !oldKeyState.IsKeyDown(Keys.D))
            {
                firing = true;
                checkShootingStartTransition(keyState, oldKeyState);
            }
            else if (!keyState.IsKeyDown(Keys.D) && oldKeyState.IsKeyDown(Keys.D))
            {
                firing = false;
                checkShootingEndTransition(keyState, oldKeyState);
            }
            else if(keyState.IsKeyDown(Keys.D) && oldKeyState.IsKeyDown(Keys.D))
            {
                if (crouching)
                    body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchShoot);
                else if (!crouching && !airborn && !keyState.IsKeyDown(Keys.Up))
                    body.setCurrentActiveSprite((int)BodySpriteIndex.Shoot);
                else if (!crouching && !airborn && keyState.IsKeyDown(Keys.Up))
                    body.setCurrentActiveSprite((int)BodySpriteIndex.ShootUp);
                else if (airborn && keyState.IsKeyDown(Keys.Down))
                    body.setCurrentActiveSprite((int)BodySpriteIndex.ShootDown);
                else if (airborn && !keyState.IsKeyDown(Keys.Down) && !keyState.IsKeyDown(Keys.Up))
                    body.setCurrentActiveSprite((int)BodySpriteIndex.Shoot);
                else if (airborn && !keyState.IsKeyDown(Keys.Down) && keyState.IsKeyDown(Keys.Up))
                    body.setCurrentActiveSprite((int)BodySpriteIndex.ShootUp);
            }

            // Magic Animation Logic //
            if (keyState.IsKeyDown(Keys.A) && !oldKeyState.IsKeyDown(Keys.A))
            {
                firingmagic = true;
                checkMagicStartTransition(keyState, oldKeyState);
            }
            else if (!keyState.IsKeyDown(Keys.A) && oldKeyState.IsKeyDown(Keys.A))
            {
                firingmagic = false;
                checkMagicEndTransition(keyState, oldKeyState);
            }
            else if (keyState.IsKeyDown(Keys.A) && oldKeyState.IsKeyDown(Keys.A))
            {
                if (crouching)
                    body.setCurrentActiveSprite((int)BodySpriteIndex.CrouchMagic);
                else if (!crouching && !airborn && !keyState.IsKeyDown(Keys.Up))
                    body.setCurrentActiveSprite((int)BodySpriteIndex.Magic);
                else if (!crouching && !airborn && keyState.IsKeyDown(Keys.Up))
                    body.setCurrentActiveSprite((int)BodySpriteIndex.MagicUp);
                else if (airborn && keyState.IsKeyDown(Keys.Down))
                    body.setCurrentActiveSprite((int)BodySpriteIndex.MagicDown);
                else if (airborn && !keyState.IsKeyDown(Keys.Down) && !keyState.IsKeyDown(Keys.Up))
                    body.setCurrentActiveSprite((int)BodySpriteIndex.Magic);
                else if (airborn && !keyState.IsKeyDown(Keys.Down) && keyState.IsKeyDown(Keys.Up))
                    body.setCurrentActiveSprite((int)BodySpriteIndex.MagicUp);
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
                    {
                        airborn = true;
                        crouching = false;
                    }
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
                for (int counter = 1; counter < movespeed; counter++)
                {
                    if (!obstacle.intersects(standingHitbox))
                    {
                        body.move(1);
                        legs.move(1);
                        bodyTransitions.move(1);
                        legsTransitions.move(1);
                    }
                    else if (obstacle.getRect().Top + 20 > standingHitbox.Bottom)
                    {
                        body.move(1);
                        legs.move(1);
                        bodyTransitions.move(1);
                        legsTransitions.move(1);
                    }
                }
            }
            else
            {
                for (int counter = 1; counter < movespeed / 2; counter++)
                {
                    if (!obstacle.intersects(standingHitbox))
                    {
                        body.move(1);
                        legs.move(1);
                        bodyTransitions.move(1);
                        legsTransitions.move(1);
                    }
                    else if (obstacle.getRect().Top + 20 > standingHitbox.Bottom)
                    {
                        body.move(1);
                        legs.move(1);
                        bodyTransitions.move(1);
                        legsTransitions.move(1);
                    }
                }
            }
            facingRight = true;
        }
        private void moveLeft(GameTime gameTime) 
        {
            if (!crouching)
            {
                for (int counter = 1; counter < movespeed; counter++)
                {
                    if (!obstacle.intersects(standingHitbox))
                    {
                        body.move(-1);
                        legs.move(-1);
                        bodyTransitions.move(-1);
                        legsTransitions.move(-1);
                    }
                    else if (obstacle.getRect().Top + 20 > standingHitbox.Bottom)
                    {
                        body.move(-1);
                        legs.move(-1);
                        bodyTransitions.move(-1);
                        legsTransitions.move(-1);
                    }
                }
            }
            else
            {
                for (int counter = 1; counter < movespeed / 2; counter++)
                {
                    if (!obstacle.intersects(standingHitbox))
                    {
                        body.move(-1);
                        legs.move(-1);
                        bodyTransitions.move(-1);
                        legsTransitions.move(-1);
                    }
                    else if (obstacle.getRect().Top + 20 > standingHitbox.Bottom)
                    {
                        body.move(-1);
                        legs.move(-1);
                        bodyTransitions.move(-1);
                        legsTransitions.move(-1);
                    }
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


        private void checkReloadTransition(KeyboardState keyState, KeyboardState oldKeyState)
        {

        }
        private void drawReloadTransition(KeyboardState keyState, KeyboardState oldKeySate)
        {

        }

        private void checkShootingStartTransition(KeyboardState keyState, KeyboardState oldKeySate)
        {
            if (crouching)
            {
                if (!oldKeySate.IsKeyDown(Keys.D))
                    drawShootingTransition("shootcrouchstart");
            }
            else if (airborn)
            {
                if (!oldKeySate.IsKeyDown(Keys.D) && keyState.IsKeyDown(Keys.Down))
                    drawShootingTransition("shootdownstart");
                else
                    drawShootingTransition("shootstart");
            }
            else if (keyState.IsKeyDown(Keys.Up))
            {
                if (!oldKeySate.IsKeyDown(Keys.D))
                    drawShootingTransition("shootupstart");
            }
            else
            {
                if (!oldKeySate.IsKeyDown(Keys.D))
                    drawShootingTransition("shootstart");
            }
            firing = true;
        }
        private void checkShootingEndTransition(KeyboardState keyState, KeyboardState oldKeySate)
        {
            if (crouching)
            {
                if (oldKeySate.IsKeyDown(Keys.D))
                    drawShootingTransition("shootcrouchend");
            }
            else if (airborn)
            {
                if (oldKeySate.IsKeyDown(Keys.D) && keyState.IsKeyDown(Keys.Down))
                    drawShootingTransition("shootdownend");
            }
            else if (keyState.IsKeyDown(Keys.Up))
            {
                if (oldKeySate.IsKeyDown(Keys.D))
                    drawShootingTransition("shootupend");
            }
            else
            {
                if (oldKeySate.IsKeyDown(Keys.D))
                    drawShootingTransition("shootend");
            }
            firing = false;
        }

       
        private void checkMagicStartTransition(KeyboardState keyState, KeyboardState oldKeyState)
        {
            if (crouching)
            {
                if (!oldKeyState.IsKeyDown(Keys.A))
                    drawShootingMagicTransition("magiccrouchstart");
            }
            else if (airborn)
            {
                if (!oldKeyState.IsKeyDown(Keys.A) && keyState.IsKeyDown(Keys.Down))
                    drawShootingTransition("magicdownstart");
                else
                    drawShootingMagicTransition("magicstart");
            }
            else if (keyState.IsKeyDown(Keys.Up))
            {
                if (!oldKeyState.IsKeyDown(Keys.A))
                    drawShootingMagicTransition("magicupstart");
            }
            else
            {
                if (!oldKeyState.IsKeyDown(Keys.A))
                    drawShootingMagicTransition("magicstart");
            }
            firingmagic = true;
        }
        private void checkMagicEndTransition(KeyboardState keyState, KeyboardState oldKeyState)
        {
            if (crouching)
            {
                if (oldKeyState.IsKeyDown(Keys.A))
                    drawShootingMagicTransition("magiccrouchend");
            }
            else if (airborn)
            {
                if (oldKeyState.IsKeyDown(Keys.A) && keyState.IsKeyDown(Keys.Down))
                    drawShootingMagicTransition("magicdownend");
            }
            else if (keyState.IsKeyDown(Keys.Up))
            {
                if (oldKeyState.IsKeyDown(Keys.A))
                    drawShootingMagicTransition("magicupend");
            }
            else
            {
                if (oldKeyState.IsKeyDown(Keys.A))
                    drawShootingMagicTransition("magicend");
            }
            firingmagic = false;
        }


        private void drawShootingTransition(string startOrEnd)
        {
            if (startOrEnd.Equals("shootstart"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootStart;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.ShootStart);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootend"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootEnd;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.ShootEnd);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootupstart"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootUpStart;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.ShootUpStart);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootupend"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootUpEnd;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.ShootUpEnd);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootdownstart"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootDownStart;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.ShootDownStart);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootdownend"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootDownEnd;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.ShootDownEnd);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootcrouchstart"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootCrouchStart;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.ShootCrouchStart);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootcrouchend"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootCrouchEnd;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.ShootCrouchEnd);
                setTransitionCheckStrings(startOrEnd);
            }
        }

        private void drawShootingMagicTransition(string startOrEnd)
        {
            if (startOrEnd.Equals("magicstart"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicStart;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicStart);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magicend"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicEnd;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicEnd);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magicupstart"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicUpStart;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicUpStart);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magicupend"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicUpEnd;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicUpEnd);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magicdownstart"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicDownStart;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicDownStart);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magicdownend"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicDownEnd;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicDownEnd);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magiccrouchstart"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicCrouchStart;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicCrouchStart);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magiccrouchend"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicCrouchEnd;
                bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicCrouchEnd);
                setTransitionCheckStrings(startOrEnd);
            }
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
            amountOfSheets = 33; // this is for the TOTAL amount of sheets that wil be loaded (this number will obviously increase over time as different animations will be needed) //
            
            imgPaths[(int)BodyTransitionIndex.Crouch] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_crouch";
            imgPaths[(int)BodyTransitionIndex.CrouchHolster] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_crouchholster";
            imgPaths[(int)BodyTransitionIndex.CrouchTurn] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_crouchturn";
            imgPaths[(int)BodyTransitionIndex.GrabLedge] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_grabledge";
            imgPaths[(int)BodyTransitionIndex.Hoist] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_hoist";
            imgPaths[(int)BodyTransitionIndex.Holster] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_holster";
            imgPaths[(int)BodyTransitionIndex.HolsterDown] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_holsterdown";
            imgPaths[(int)BodyTransitionIndex.HolsterUp] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_holsterup";
            imgPaths[(int)BodyTransitionIndex.LetGo] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_letgo";
            imgPaths[(int)BodyTransitionIndex.LowToMid] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_lowtomid";
            imgPaths[(int)BodyTransitionIndex.MagicCrouchEnd] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_magiccrouchend";
            imgPaths[(int)BodyTransitionIndex.MagicCrouchStart] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_magiccrouchstart";
            imgPaths[(int)BodyTransitionIndex.MagicDownEnd] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_magicdownend";
            imgPaths[(int)BodyTransitionIndex.MagicDownStart] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_magicdownstart";
            imgPaths[(int)BodyTransitionIndex.MagicEnd] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_magicend";
            imgPaths[(int)BodyTransitionIndex.MagicStart] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_magicstart";
            imgPaths[(int)BodyTransitionIndex.MagicUpEnd] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_magicupend";
            imgPaths[(int)BodyTransitionIndex.MagicUpStart] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_magicupstart";
            imgPaths[(int)BodyTransitionIndex.MidToLow] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_midtolow";
            imgPaths[(int)BodyTransitionIndex.MidToUp] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_midtoup";
            imgPaths[(int)BodyTransitionIndex.ShootCrouchEnd] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_shootcrouchend";
            imgPaths[(int)BodyTransitionIndex.ShootCrouchStart] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_shootcrouchstart";
            imgPaths[(int)BodyTransitionIndex.ShootDownEnd] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_shootdownend";
            imgPaths[(int)BodyTransitionIndex.ShootDownStart] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_shootdownstart";
            imgPaths[(int)BodyTransitionIndex.ShootEnd] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_shootend";
            imgPaths[(int)BodyTransitionIndex.ShootStart] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_shootstart";
            imgPaths[(int)BodyTransitionIndex.ShootUpEnd] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_shootupend";
            imgPaths[(int)BodyTransitionIndex.ShootUpStart] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_shootupstart";
            imgPaths[(int)BodyTransitionIndex.Turn] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_turn";
            imgPaths[(int)BodyTransitionIndex.TurnDown] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_turndown"; 
            imgPaths[(int)BodyTransitionIndex.TurnUp] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_turnup";
            imgPaths[(int)BodyTransitionIndex.UnCrouch] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_uncrouch";
            imgPaths[(int)BodyTransitionIndex.UpToMid] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_uptomid";

            frameAmounts[(int)BodyTransitionIndex.Crouch] = 6;
            frameAmounts[(int)BodyTransitionIndex.CrouchHolster] = 8;
            frameAmounts[(int)BodyTransitionIndex.CrouchTurn] = 4;
            frameAmounts[(int)BodyTransitionIndex.GrabLedge] = 9;
            frameAmounts[(int)BodyTransitionIndex.Hoist] = 14;
            frameAmounts[(int)BodyTransitionIndex.Holster] = 8;
            frameAmounts[(int)BodyTransitionIndex.HolsterDown] = 8;
            frameAmounts[(int)BodyTransitionIndex.HolsterUp] = 8;
            frameAmounts[(int)BodyTransitionIndex.LetGo] = 3;
            frameAmounts[(int)BodyTransitionIndex.LowToMid] = 4;
            frameAmounts[(int)BodyTransitionIndex.MagicCrouchEnd] = 4;
            frameAmounts[(int)BodyTransitionIndex.MagicCrouchStart] = 3;
            frameAmounts[(int)BodyTransitionIndex.MagicDownEnd] = 5;
            frameAmounts[(int)BodyTransitionIndex.MagicDownStart] = 3;
            frameAmounts[(int)BodyTransitionIndex.MagicEnd] = 6;
            frameAmounts[(int)BodyTransitionIndex.MagicStart] = 3;
            frameAmounts[(int)BodyTransitionIndex.MagicUpEnd] = 5;
            frameAmounts[(int)BodyTransitionIndex.MagicUpStart] = 3;
            frameAmounts[(int)BodyTransitionIndex.MidToLow] = 4;
            frameAmounts[(int)BodyTransitionIndex.MidToUp] = 4;
            frameAmounts[(int)BodyTransitionIndex.ShootCrouchEnd] = 6;
            frameAmounts[(int)BodyTransitionIndex.ShootCrouchStart] = 2;
            frameAmounts[(int)BodyTransitionIndex.ShootDownEnd] = 6;
            frameAmounts[(int)BodyTransitionIndex.ShootDownStart] = 2;
            frameAmounts[(int)BodyTransitionIndex.ShootEnd] = 6;
            frameAmounts[(int)BodyTransitionIndex.ShootStart] = 2;
            frameAmounts[(int)BodyTransitionIndex.ShootUpEnd] = 6;
            frameAmounts[(int)BodyTransitionIndex.ShootUpStart] = 2;
            frameAmounts[(int)BodyTransitionIndex.Turn] = 4;
            frameAmounts[(int)BodyTransitionIndex.TurnDown] = 4;
            frameAmounts[(int)BodyTransitionIndex.TurnUp] = 4;
            frameAmounts[(int)BodyTransitionIndex.UnCrouch] = 6;
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
            frameAmounts = new int[35]; 
            frameRects = new Rectangle[35];
            imgPaths = new string[35];
            //startingPos = new Vector2[20];
            spriteDelayTimes = new int[35];
        }

        public void passPlatform(Platform plat, Platform obs)
        {
            floor = plat;
            obstacle = obs;
        } // for debugging collision & jumping //
    }
}
