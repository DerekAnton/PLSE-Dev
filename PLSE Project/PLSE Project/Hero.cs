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
    
    public enum BodySpriteIndex { CrouchIdle, CrouchMagic, CrouchReload, CrouchShoot, Fall, GetHurt, HangIdle, Idle, IdleDown, IdleUp, Jump, Magic, MagicDown, MagicUp, PullTowards, PushAway, Reload, ReloadUp, Shoot, ShootDown, ShootUp }; 
    public enum LegSpriteIndex { CrouchWalk, Fall, GetHurt, Idle, Jump, Running, CrouchIdle  };

    public enum BodyTransitionIndex
    {
        Crouch, CrouchHolster, /*new*/CrouchReload, CrouchTurn, /*new*/Fall, /*new*/GetHurt, GrabLedge, Hoist, Holster, HolsterDown, HolsterUp, /*new*/Jump, LetGo, LowToMid,
        MagicCrouchEnd, MagicCrouchStart, MagicDownEnd, MagicDownStart, MagicEnd, MagicStart, MagicUpEnd, MagicUpStart, MidToLow, MidToUp, /*new*/Reload, /*new*/ReloadUp,
        ShootCrouchEnd, ShootCrouchStart, ShootDownEnd, ShootDownStart, ShootEnd, ShootStart, ShootUpEnd, ShootUpStart, Turn, TurnDown, 
        TurnUp, UnCrouch, UpToMid, NULL };

    public enum LegTransitionIndex { Crouch, Fall, GetHurt, Jump, TwistJump, UnCrouch, NULL };

    public enum HeroStates { StandStill, CrouchStill, CrouchWalking, Running, Jumping};

    class Hero : Colideable
    {

        private static double health = 8;
        private static double maxHealth = 1000;
        private static double energy = 100;
        private static double maxEnergy = 100;

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
        public static Rectangle standingHitbox = new Rectangle(50, 50, 70, 145); 
        public static Rectangle crouchingHitbox = new Rectangle(50, 75, 70, 72);

        public static Rectangle activeHitbox; // to hold either crouching or standing hitbox //

        public static Weapon[] weaponArsenal = new Weapon[3];
        public static WeaponTransition[] weaponTransitionArsenal = new WeaponTransition[3];
        public static int currentActiveWeapon = (int)CurrentWeapon.Pistol;

        public static BodyPart legs;
        //BodyPart singleSheet; // the single sheet body part is important for crouching & hanging since the legs/body are one full piece //
        //public static TransitionBodyPart bodyTransitions; // the transition body part will break up the sprite sheets that have to do with changing a state i.e. from idle to crouching needs an quick animation played over the hero to transition it smoothly.//
        public static TransitionBodyPart legsTransitions;

        private bool animateHoldLastFrame = false; // used for a body part aniamtion that is specific to the special sprites that animate one time but hold on the last frame. //

        private static HeroStates heroState = HeroStates.StandStill;

        static public LegTransitionIndex currentLegTransition = LegTransitionIndex.NULL; // these two values will be stored with the corresponding int val of whatever transition animation needs to be played //
        static public BodyTransitionIndex currentBodyTransition = BodyTransitionIndex.NULL; // //
        static public bool singleAnimationLock = true;
       
        public static bool spriteFlipping = false; 
        bool crouching = false;

        private bool isStandingHitbox = true; // only two types of hit boxes, standing and crouching, (crouching == !standing) //
        private bool offsetCheck = false;
        private bool facingRight = true;
        bool facingUp = false;

        private bool movementlock = false; // used specificly to choose a direction when the player hits down, right, and left all at the same time (so we dont end up adding and subtracting a movement in the air, and effectivley moving nowhere)

        Rectangle pointerHolder; // debug

        public static bool reloading = false;

        static Rectangle newHitbox;

        public Hero() { }

        public void load(ContentManager content, int x, int y)
        {
            initalizeStagingArrays();

            setLegHardCodedVals();
            legs = new BodyPart(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);

            setLegsTransitionHardCodedValues();
            legsTransitions = new TransitionBodyPart(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);

            loadArsenal(content);

            checkCrouchingHitbox();

            legs.setCurrentActiveSprite((int)LegSpriteIndex.Idle);
            setCurrentActiveWeaponSprite((int)BodySpriteIndex.Idle);
        }

        public void update(double elapsedTime, KeyboardState keyState, KeyboardState oldKeyState, Rectangle viewportRect, GameTime gameTime, ContentManager content)
        {
            
            newHitbox = new Rectangle(activeHitbox.X, activeHitbox.Y, activeHitbox.Width, activeHitbox.Height);

            if(keyState.IsKeyDown(Keys.Down) && !airborn)
                newHitbox = new Rectangle(activeHitbox.X, activeHitbox.Y, activeHitbox.Width, activeHitbox.Height/2);

 
           /* foreach (Rectangle obstacle in ObstacleManager.getColisionRectangles())
            {
                Console.WriteLine(obstacle.Intersects(activeHitbox) || obstacle.Contains(activeHitbox));
                Console.Out.WriteLine("(" + activeHitbox.X + ", " + activeHitbox.Y + ")");
            }
            */
            handleFalling(gameTime);
            checkCrouchingHitbox();


            if (firingmagic)
                Hero.changeEnergy(-1);
            else
                Hero.changeEnergy(2);

            if (!(currentBodyTransition == BodyTransitionIndex.NULL))
            {
                weaponTransitionArsenal[currentActiveWeapon].animateUntilEndFrame(gameTime, content);

                if (!(currentLegTransition == LegTransitionIndex.NULL))
                    legsTransitions.animateUntilEndFrame(gameTime, content);
                else
                {
                    legs.animate(gameTime, content, keyState);
                }
            }
            else if (!(currentLegTransition == LegTransitionIndex.NULL))
            {
                legsTransitions.animateUntilEndFrame(gameTime, content);

                if (!(currentBodyTransition == BodyTransitionIndex.NULL))
                {
                    weaponTransitionArsenal[currentActiveWeapon].animateUntilEndFrame(gameTime, content);
                }
                else
                {
                    for (int counter = 0; counter < 3; counter++)
                    {
                        weaponArsenal[counter].animate(gameTime, content, keyState);
                    }
                }
            }
            else
            {
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponArsenal[counter].animate(gameTime, content, keyState);
                }
                legs.animate(gameTime, content, keyState);
            }
            movementlock = false;
            activeHitbox.X = newHitbox.X;
            activeHitbox.Y = newHitbox.Y;
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

                    weaponTransitionArsenal[currentActiveWeapon].draw(spriteBatch, spriteFlipping);

                }
                else if (!(currentLegTransition == LegTransitionIndex.NULL))
                {
                    legsTransitions.draw(spriteBatch, spriteFlipping);

                    if (!(currentBodyTransition == BodyTransitionIndex.NULL))
                    {
                        weaponTransitionArsenal[currentActiveWeapon].draw(spriteBatch, spriteFlipping);
                    }
                    else
                    {
                        for (int counter = 0; counter < 3; counter++)
                        {
                            if (counter == currentActiveWeapon)
                                weaponArsenal[counter].draw(spriteBatch, spriteFlipping);
                        }
                    }
                }
                else
                {
                    legs.draw(spriteBatch, spriteFlipping);
                    for (int counter = 0; counter < 3; counter++)
                    {
                        if (counter == currentActiveWeapon)
                            weaponArsenal[counter].draw(spriteBatch, spriteFlipping);
                    }
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

                    weaponTransitionArsenal[currentActiveWeapon].draw(spriteBatch, spriteFlipping);
                }
                else if (!(currentLegTransition == LegTransitionIndex.NULL))
                {
                    legsTransitions.draw(spriteBatch, spriteFlipping);

                    if (!(currentBodyTransition == BodyTransitionIndex.NULL))
                    {
                        weaponTransitionArsenal[currentActiveWeapon].draw(spriteBatch, spriteFlipping);
                    }
                    else
                    {
                        for (int counter = 0; counter < 3; counter++)
                        {
                            if (counter == currentActiveWeapon)
                                weaponArsenal[counter].draw(spriteBatch, spriteFlipping);
                        }
                    }
                }
                else
                {
                    legs.draw(spriteBatch, spriteFlipping);
                    for (int counter = 0; counter < 3; counter++)
                    {
                        if (counter == currentActiveWeapon)
                            weaponArsenal[counter].draw(spriteBatch, spriteFlipping);
                    }
                }
            }
        }

        private void move(KeyboardState keyState, KeyboardState oldKeyState, Rectangle viewportRect, double elapsedTime, GameTime gameTime)
        {

            if (keyState.IsKeyDown(Keys.LeftShift) && !oldKeyState.IsKeyDown(Keys.LeftShift)) // testing weapon switching // 
            {
                changeWeapon(keyState, oldKeyState);
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
                for (int counter = 0; counter < 3; counter++)
                    weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                setCurrentActiveWeaponSprite((int)BodySpriteIndex.CrouchIdle);
                legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                setCheckStrings("crouchidle");
            }
            
            // Hero Idle //
            if ( !keyState.IsKeyDown(Keys.Down) &&  (!keyState.IsKeyDown(Keys.Right) || !keyState.IsKeyDown(Keys.Left))) 
            {
                crouching = false;
                heroState = HeroStates.StandStill;
                for (int counter = 0; counter < 3; counter++)
                    weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.Idle);
                setCurrentActiveWeaponSprite((int)BodySpriteIndex.Idle);
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
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                    }
                    else if (!keyState.IsKeyDown(Keys.Left) && !airborn)                                 // condition for crouched movement (right) on ground//
                    {
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchWalk);
                        moveRight(gameTime);
                    }
                    else if (keyState.IsKeyDown(Keys.Left) && airborn)                                   // right down left and airborn //
                    {
                        crouching = false;
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.IdleDown);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.IdleDown);
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
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.IdleDown);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.IdleDown);
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
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.Idle);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.Idle);
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
                    
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                    }
                    else if (keyState.IsKeyDown(Keys.Down) && airborn)                                   // right left down & airborn// 
                    {
                        crouching = false;
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.IdleDown);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.IdleDown);
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

                    for (int counter = 0; counter < 3; counter++)
                        weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.Idle);
                    setCurrentActiveWeaponSprite((int)BodySpriteIndex.Idle);
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
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchWalk);
                        moveLeft(gameTime);
                    }
                    if(keyState.IsKeyDown(Keys.Right) && !airborn)                                 // Left Down Right and Not Airborn// 
                    {
                        crouching = true;
                        heroState = HeroStates.CrouchStill;
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);

                    }
                    if (!keyState.IsKeyDown(Keys.Right) && airborn)                                // Left Down Jumping //
                    {
                        crouching = false;
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.IdleDown);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.IdleDown);
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
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.IdleDown);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.IdleDown);
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
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.Idle);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.Idle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.Idle);
                    }
                    else if (keyState.IsKeyDown(Keys.Down) && !airborn)                                 // Right Left Down Grounded //
                    {
                        crouching = true;
                        heroState = HeroStates.CrouchStill;
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.CrouchIdle);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.CrouchIdle);
                        legs.setCurrentActiveSprite((int)LegSpriteIndex.CrouchIdle);
                    }
                    else if (keyState.IsKeyDown(Keys.Down) && airborn)                                  // Left Right Down airborn //
                    {
                        crouching = true;
                        heroState = HeroStates.CrouchStill;
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.CrouchIdle);
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
                    for (int counter = 0; counter < 3; counter++)
                        weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.Idle);
                    setCurrentActiveWeaponSprite((int)BodySpriteIndex.Idle);
                    legs.setCurrentActiveSprite((int)LegSpriteIndex.Running);

                    checkMidToUpConditions(keyState, oldKeyState);
                    checkUpToMidConditions(keyState, oldKeyState);
                }
            }
            if (keyState.IsKeyDown(Keys.Down) && (!keyState.IsKeyDown(Keys.Right) && !keyState.IsKeyDown(Keys.Left)) && airborn) // crouched, and in the air (no right or left) //
            {
                for (int counter = 0; counter < 3; counter++)
                    weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.IdleDown);
                setCurrentActiveWeaponSprite((int)BodySpriteIndex.IdleDown);
                legs.setCurrentActiveSprite((int)LegSpriteIndex.Jump);
            }

            // Reload //
            if (keyState.IsKeyDown(Keys.S) && !oldKeyState.IsKeyDown(Keys.S))
            {
                checkReloadTransition(keyState, oldKeyState);
                switch (getCurrentWeapon())
                {
                    case CurrentWeapon.Pistol:
                        Weapon.reloadPistol();
                        break;
                    case CurrentWeapon.Rifle:
                        Weapon.reloadRifle();
                        break;
                    case CurrentWeapon.Rocket:
                        Weapon.reloadRckt();
                        break;
                }
            }

            // Auto-Reload System //
            if (currentActiveWeapon == (int)CurrentWeapon.Pistol)
            {
                if (Weapon.pistolclipEmpty && !crouching)
                {
                    if (keyState.IsKeyDown(Keys.Up))
                        weaponTransitionArsenal[(int)CurrentWeapon.Pistol].setCurrentActiveSprite((int)BodyTransitionIndex.ReloadUp);
                    else
                        weaponTransitionArsenal[(int)CurrentWeapon.Pistol].setCurrentActiveSprite((int)BodyTransitionIndex.Reload);
                    Weapon.reloadPistol();
                }
                else if (Weapon.pistolclipEmpty && crouching)
                {
                    weaponTransitionArsenal[(int)CurrentWeapon.Pistol].setCurrentActiveSprite((int)BodyTransitionIndex.CrouchReload);
                    Weapon.reloadPistol();
                }
            }
            else if (currentActiveWeapon == (int)CurrentWeapon.Rifle)
            {
                if (Weapon.rifleclipEmpty && !crouching)
                {
                    if (keyState.IsKeyDown(Keys.Up))
                        weaponTransitionArsenal[(int)CurrentWeapon.Rifle].setCurrentActiveSprite((int)BodyTransitionIndex.ReloadUp);
                    else
                        weaponTransitionArsenal[(int)CurrentWeapon.Rifle].setCurrentActiveSprite((int)BodyTransitionIndex.Reload);
                    Weapon.reloadRifle();
                }
                else if (Weapon.rifleclipEmpty && crouching)
                {
                    weaponTransitionArsenal[(int)CurrentWeapon.Rifle].setCurrentActiveSprite((int)BodyTransitionIndex.CrouchReload);
                    Weapon.reloadRifle();
                }
            }

            else if (currentActiveWeapon == (int)CurrentWeapon.Rocket)
            {
                if (Weapon.rcktclipEmpty && !crouching)
                {
                    if (keyState.IsKeyDown(Keys.Up))
                        weaponTransitionArsenal[(int)CurrentWeapon.Rocket].setCurrentActiveSprite((int)BodyTransitionIndex.ReloadUp);
                    else
                        weaponTransitionArsenal[(int)CurrentWeapon.Rocket].setCurrentActiveSprite((int)BodyTransitionIndex.Reload);
                    Weapon.reloadRckt();
                }
                else if (Weapon.rcktclipEmpty && crouching)
                {
                    weaponTransitionArsenal[(int)CurrentWeapon.Rocket].setCurrentActiveSprite((int)BodyTransitionIndex.CrouchReload);
                    Weapon.reloadRckt();
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
            else if (keyState.IsKeyDown(Keys.D) && oldKeyState.IsKeyDown(Keys.D))
            {
                if (crouching)
                {
                    if (currentActiveWeapon == (int)CurrentWeapon.Rifle)
                    {
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.CrouchShoot);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.CrouchShoot);
                    }
                }
                else if (!crouching && !airborn && !keyState.IsKeyDown(Keys.Up))
                {
                    if (currentActiveWeapon == (int)CurrentWeapon.Rifle)
                    {
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.Shoot);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.Shoot);
                    }
                }
                else if (!crouching && !airborn && keyState.IsKeyDown(Keys.Up))
                {
                    if (currentActiveWeapon == (int)CurrentWeapon.Rifle)
                    {
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.ShootUp);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.ShootUp);
                    }
                }
                else if (airborn && keyState.IsKeyDown(Keys.Down))
                {
                    if (currentActiveWeapon == (int)CurrentWeapon.Rifle)
                    {
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.ShootDown);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.ShootDown);
                    }
                }
                else if (airborn && !keyState.IsKeyDown(Keys.Down) && !keyState.IsKeyDown(Keys.Up))
                {
                    if (currentActiveWeapon == (int)CurrentWeapon.Rifle)
                    {
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.Shoot);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.Shoot);
                    }
                }
                else if (airborn && !keyState.IsKeyDown(Keys.Down) && keyState.IsKeyDown(Keys.Up))
                {
                    if (currentActiveWeapon == (int)CurrentWeapon.Rifle)
                    {
                        for (int counter = 0; counter < 3; counter++)
                            weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.ShootUp);
                        setCurrentActiveWeaponSprite((int)BodySpriteIndex.ShootUp);
                    }
                }
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
                {
                    for (int counter = 0; counter < 3; counter++)
                        weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.CrouchMagic);
                    setCurrentActiveWeaponSprite((int)BodySpriteIndex.CrouchMagic);
                }
                else if (!crouching && !airborn && !keyState.IsKeyDown(Keys.Up))
                {
                    for (int counter = 0; counter < 3; counter++)
                        weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.Magic);
                    setCurrentActiveWeaponSprite((int)BodySpriteIndex.Magic);
                }
                else if (!crouching && !airborn && keyState.IsKeyDown(Keys.Up))
                {
                    for (int counter = 0; counter < 3; counter++)
                        weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.MagicUp);
                    setCurrentActiveWeaponSprite((int)BodySpriteIndex.MagicUp);
                }
                else if (airborn && keyState.IsKeyDown(Keys.Down))
                {
                    for (int counter = 0; counter < 3; counter++)
                        weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.MagicDown);
                    setCurrentActiveWeaponSprite((int)BodySpriteIndex.MagicDown);
                }
                else if (airborn && !keyState.IsKeyDown(Keys.Down) && !keyState.IsKeyDown(Keys.Up))
                {
                    for (int counter = 0; counter < 3; counter++)
                        weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.Magic);
                    setCurrentActiveWeaponSprite((int)BodySpriteIndex.Magic);
                }
                else if (airborn && !keyState.IsKeyDown(Keys.Down) && keyState.IsKeyDown(Keys.Up))
                {
                    for (int counter = 0; counter < 3; counter++)
                        weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.MagicUp);
                    setCurrentActiveWeaponSprite((int)BodySpriteIndex.MagicUp);
                }
            }

            // Jumping Logic // 
            if (keyState.IsKeyDown(Keys.Space) && !oldKeyState.IsKeyDown(Keys.Space))
            {
                if (jumpsLeft <= 2 && jumpsLeft > 0)
                {
                    heroState = HeroStates.Jumping;
                    for (int counter = 0; counter < 3; counter++)
                        weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.Jump);
                    setCheckStrings("jumping");
                    jump();
                }
            }
            if (airborn)
                legs.setCurrentActiveSprite((int)LegSpriteIndex.Fall);
        }


        private bool checkIfReloading()
        {
            if (weaponTransitionArsenal[(int)getCurrentWeapon()].getCurrentActiveSprite() == (int)BodyTransitionIndex.Reload || weaponTransitionArsenal[(int)getCurrentWeapon()].getCurrentActiveSprite() == (int)BodyTransitionIndex.ReloadUp || weaponTransitionArsenal[(int)getCurrentWeapon()].getCurrentActiveSprite() == (int)BodyTransitionIndex.CrouchReload && currentBodyTransition != BodyTransitionIndex.NULL)
            {
                reloading = true;
                return true;
            }
            else
            {
                reloading = false;
                return false;
            }
            
        }

        private void handleFalling(GameTime gameTime)
        {
            frameLimiter += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (frameLimiter >= MILLISECOND_DELAY)
            {
                frameLimiter = 0;
                if (jumpAcceleration <= 0 && checkLevelCollision(activeHitbox))
                {
                    falling = false;
                    airborn = false;
                    doubleJumping = false;
                    animateHoldLastFrame = false;
                    jumpAcceleration = 0;
                    jumpsLeft = 2;
                }
                else
                {
                    if (!falling && jumpAcceleration < 0) // highest point of jump
                    {
                        falling = true;
                    }
                    if (!airborn)
                    {
                        airborn = true;
                        animateHoldLastFrame = true;
                        crouching = false;
                    }
                    if (jumpAcceleration < -40)
                        jumpAcceleration = -40;
                    else if (jumpAcceleration > -40)
                        jumpAcceleration -= 3;

                    int absJumpAccel = Math.Abs(jumpAcceleration);
                    for (int counter = 0; counter < absJumpAccel; counter++)
                    {
                        if (jumpAcceleration <= 0 && checkLevelCollision(activeHitbox))
                            break;
                        if (jumpAcceleration > 0)
                        {
                            CameraManager.changeYOffset(1);
                            updateHitboxes();
                        }
                        else
                        {
                           CameraManager.changeYOffset(-1);
                           updateHitboxes();
                        }
                    }
                }
            }
            
                bool check = false;
                foreach (Rectangle obstacle in ObstacleManager.getColisionRectangles())
                {
                    if ((obstacle.Top < activeHitbox.Bottom - 5) && obstacle.Intersects(activeHitbox))
                    {
                        if (activeHitbox.Top < obstacle.Top - 20)
                        {
                            check = true;
                            pointerHolder = obstacle;
                        }
                    }
                }
                if (check)
                {
                    CameraManager.changeYOffset(5);
                    check = false;
                }


                bool checktwo = false;
                foreach (Rectangle obstacle in ObstacleManager.getColisionRectangles())
                {
                    if ((activeHitbox.Top < obstacle.Bottom + 5) && obstacle.Intersects(activeHitbox))
                    {
                            checktwo = true;
                            pointerHolder = obstacle;
                    }
                }
                if (checktwo)
                {
                    CameraManager.changeYOffset(-5);
                    checktwo = false;
                }

        }

        private bool checkLevelCollision(Rectangle hitbox)
        {
            for (int counter = 0; counter < ObstacleManager.getColisionRectangles().Length; counter++)
            {
                 if (ObstacleManager.getColisionRectangles()[counter].Intersects(hitbox))
                     return true;
            }
            return false;
        }

        private void setLegsOffsetFalse()
        {
            if (offsetCheck)
            {
                legs.subtXOffset(2);
                for (int i = 0; i < 3; i++)
                {
                    weaponArsenal[i].subtXOffset(-55);
                    weaponTransitionArsenal[i].subtXOffset(-55);
                }
                legsTransitions.subtXOffset(2);
                offsetCheck = false;
            }
        }
        private void setLegsOffsetTrue()
        {
            if (!offsetCheck)
            {
                legs.addXOffset(2);
                for (int i = 0; i < 3; i++)
                {
                    weaponArsenal[i].addXOffset(-55);
                    weaponTransitionArsenal[i].addXOffset(-55);
                }
                legsTransitions.addXOffset(2);
                offsetCheck = true;
            }
        }

        private void setCheckStrings(string newString)
        {
            if (!bodyString.Equals(newString))
            {
                bodyString = newString;
                for (int i = 0; i < 3; i++)
                {
                    weaponArsenal[i].resetAnimationValues();
                }
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
                for (int counter = 0; counter < 3; counter++)
                    weaponTransitionArsenal[currentActiveWeapon].resetAnimationValues();
            }
            if (!legString.Equals(newString))
            {
                legString = newString;
                legsTransitions.resetAnimationValues();
            }
        }

        public static void setX(int x)
        {
            legs.setXPos(x - 51);
            legsTransitions.setXPos(x - 51);

            for (int counter = 0; counter < 3; counter++)
            {
                weaponArsenal[counter].setXPos(x);
                weaponTransitionArsenal[counter].setXPos(x);
            }

        }
        public static void setY(int y)
        {
            legs.setYPos(y + 78);
            legsTransitions.setYPos(y + 78);
            
            for (int counter = 0; counter < 3; counter++)
            {
                weaponArsenal[counter].setYPos(y);
                weaponTransitionArsenal[counter].setYPos(y);
            }

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
            if (energy < 0)
                energy = 0;
            if (energy > maxEnergy)
                energy = maxEnergy;
        }
        public static void dealHeroDmg(double dmgToBeDealt)
        {
            health -= dmgToBeDealt;
        }


        private void changeWeapon(KeyboardState keyState, KeyboardState oldKeyState)
        {
            currentActiveWeapon++;
            if (currentActiveWeapon >= 3)
                currentActiveWeapon = 0;
            checkChangeWeaponTransition(keyState, oldKeyState);
        }

        private void jump()
        {
            jumpAcceleration = 32;
            jumpsLeft--;
        }

        public bool intersects(Rectangle rectangle)
        {
            return activeHitbox.Intersects(rectangle);
        }
        public bool intersects(Colideable obj)
        {
            return activeHitbox.Intersects(obj.getRect());
        }
        public Rectangle getRect()
        {

            if (isStandingHitbox)
                return newHitbox;
            else
                return crouchingHitbox;
        }
        
        private void moveRight(GameTime gameTime) 
        {
            bool secCheck = true;
            
            foreach (Rectangle obstacle in ObstacleManager.getColisionRectangles())
            {
                if ((activeHitbox.Right >= obstacle.Left) && (activeHitbox.Right < obstacle.Left + 5) && (obstacle.Intersects(activeHitbox)))
                    secCheck = false;
            }
            if (secCheck)
            {
                if(!crouching)
                    CameraManager.changeXOffset(-movespeed); // move right
                else
                    CameraManager.changeXOffset(-movespeed/2); // move right
                secCheck = true;
            }
            facingRight = true;




        }
        private void moveLeft(GameTime gameTime) 
        {
            bool secCheck = true;

            foreach (Rectangle obstacle in ObstacleManager.getColisionRectangles())
            {
                if ((activeHitbox.Left <= obstacle.Right) && (activeHitbox.Left > obstacle.Right - 10) && (obstacle.Intersects(activeHitbox)))
                    secCheck = false;
            }
            if (secCheck)
            {
                if(!crouching)
                    CameraManager.changeXOffset(movespeed);
                else
                    CameraManager.changeXOffset(movespeed / 2); // move right
                secCheck = false;
            }
            facingRight = false;
        }

        public void updateHitboxes()
        {
            if (!crouching && facingRight)
            {
                activeHitbox.X = (int)weaponArsenal[((int)CurrentWeapon.Pistol)].position.X - 70;
                activeHitbox.Y = (int)weaponArsenal[((int)CurrentWeapon.Pistol)].position.Y - 43;
            }
            else if (crouching && facingRight)
            {
                activeHitbox.X = (int)weaponArsenal[((int)CurrentWeapon.Pistol)].position.X - 70;
                activeHitbox.Y = (int)weaponArsenal[((int)CurrentWeapon.Pistol)].position.Y - 43;
            }
            else if (!crouching && !facingRight)
            {
                activeHitbox.X = (int)weaponArsenal[((int)CurrentWeapon.Pistol)].position.X - 25;
                activeHitbox.Y = (int)weaponArsenal[((int)CurrentWeapon.Pistol)].position.Y - 43;
            }
            else if (crouching && !facingRight)
            {
                activeHitbox.X = (int)weaponArsenal[((int)CurrentWeapon.Pistol)].position.X - 25;
                activeHitbox.Y = (int)weaponArsenal[((int)CurrentWeapon.Pistol)].position.Y - 43;
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


        private void drawJumpingTransition(string checkString)
        {
            if (checkString.Equals("jump"))
            {
                //legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                currentBodyTransition = BodyTransitionIndex.Jump;
                currentLegTransition = LegTransitionIndex.Jump;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.Jump);
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                legsTransitions.setCurrentActiveSprite((int)LegTransitionIndex.Jump);
                setTransitionCheckStrings(checkString);
            }
            else if (checkString.Equals("fall"))
            {
                //legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                currentBodyTransition = BodyTransitionIndex.Fall;
                currentLegTransition = LegTransitionIndex.Fall;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.Fall);
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                legsTransitions.setCurrentActiveSprite((int)LegTransitionIndex.Fall);
                setTransitionCheckStrings(checkString);
            }
        }


        private void checkChangeWeaponTransition(KeyboardState keyState, KeyboardState oldKeyState)
        {
            if (crouching)
            {
                if (!oldKeyState.IsKeyDown(Keys.LeftShift))
                    drawChangeWeaponTransition("crouchholster");
            }
            else if (airborn)
            {
                if (!oldKeyState.IsKeyDown(Keys.LeftShift) && keyState.IsKeyDown(Keys.Down))
                    drawChangeWeaponTransition("holsterdown");
                else
                    drawChangeWeaponTransition("holster");
            }
            else if (keyState.IsKeyDown(Keys.Up))
            {
                if (!oldKeyState.IsKeyDown(Keys.LeftShift))
                    drawChangeWeaponTransition("holsterup");
            }
            else
            {
                if (!oldKeyState.IsKeyDown(Keys.LeftShift))
                    drawChangeWeaponTransition("holster");
            }
        }
        private void drawChangeWeaponTransition(string checkString)
        {
            if (checkString.Equals("crouchholster"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                currentBodyTransition = BodyTransitionIndex.CrouchHolster;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.CrouchHolster);
                setTransitionCheckStrings(checkString);
            }
            else if (checkString.Equals("holster"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                currentBodyTransition = BodyTransitionIndex.Holster;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.Holster);
                setTransitionCheckStrings(checkString);
            }
            else if (checkString.Equals("holsterdown"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                currentBodyTransition = BodyTransitionIndex.HolsterDown;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.HolsterDown);
                setTransitionCheckStrings(checkString);
            }
            else if (checkString.Equals("holsterup"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                currentBodyTransition = BodyTransitionIndex.HolsterUp;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.HolsterUp);
                setTransitionCheckStrings(checkString);
            }
        }

        private void checkReloadTransition(KeyboardState keyState, KeyboardState oldKeyState)
        {
            if (crouching)
            {
                if (!oldKeyState.IsKeyDown(Keys.S))
                    drawReloadTransition("crouchreload");
            }
            else if (keyState.IsKeyDown(Keys.Up))
            {
                if (!oldKeyState.IsKeyDown(Keys.S))
                    drawReloadTransition("reloadup");
            }
            else
            {
                if (!oldKeyState.IsKeyDown(Keys.S))
                    drawReloadTransition("reload");
            }
        }
        private void drawReloadTransition(string checkString)
        {
            if (checkString.Equals("crouchreload"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                currentBodyTransition = BodyTransitionIndex.CrouchReload; 
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.CrouchReload);
                setTransitionCheckStrings(checkString);
            }
            else if (checkString.Equals("reload"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                currentBodyTransition = BodyTransitionIndex.Reload; 
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.Reload);
                setTransitionCheckStrings(checkString);
            }
            else if (checkString.Equals("reloadup"))
            {
                legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
                currentBodyTransition = BodyTransitionIndex.ReloadUp; 
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.ReloadUp);
                setTransitionCheckStrings(checkString);
            }
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
                if (oldKeySate.IsKeyDown(Keys.D) && currentBodyTransition == BodyTransitionIndex.NULL)
                    drawShootingTransition("shootcrouchend");
            }
            else if (airborn)
            {
                if (oldKeySate.IsKeyDown(Keys.D) && keyState.IsKeyDown(Keys.Down) && currentBodyTransition == BodyTransitionIndex.NULL)
                    drawShootingTransition("shootdownend");
            }
            else if (keyState.IsKeyDown(Keys.Up))
            {
                if (oldKeySate.IsKeyDown(Keys.D) && currentBodyTransition == BodyTransitionIndex.NULL)
                    drawShootingTransition("shootupend");
            }
            else
            {
                if (oldKeySate.IsKeyDown(Keys.D) && currentBodyTransition == BodyTransitionIndex.NULL) // BREAK THROUGH //
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
               // legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootStart; // BREAK THROUGH //
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.ShootStart);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootend"))
            {
               // legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootEnd;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.ShootEnd);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootupstart"))
            {
               // legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootUpStart;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.ShootUpStart);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootupend"))
            {
               // legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootUpEnd;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.ShootUpEnd);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootdownstart"))
            {
               // legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootDownStart;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.ShootDownStart);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootdownend"))
            {
               // legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootDownEnd;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.ShootDownEnd);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootcrouchstart"))
            {
               // legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootCrouchStart;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.ShootCrouchStart);
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("shootcrouchend"))
            {
              //  legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.ShootCrouchEnd;
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.ShootCrouchEnd);
                setTransitionCheckStrings(startOrEnd);
            }
        }
        private void drawShootingMagicTransition(string startOrEnd)
        {
            if (startOrEnd.Equals("magicstart"))
            {
              //  legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
             //   bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicStart;
             //   bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicStart);
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                    weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.MagicStart);
                }
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magicend"))
            {
              //  legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
             //   bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicEnd;
             //   bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicEnd);
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                    weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.MagicEnd);
                }
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magicupstart"))
            {
              //  legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
             //   bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicUpStart;
            //    bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicUpStart);
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                    weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.MagicUpStart);
                }
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magicupend"))
            {
            //    legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
           //     bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicUpEnd;
           //     bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicUpEnd);
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                    weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.MagicUpEnd);
                }
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magicdownstart"))
            {
            //    legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
            //    bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicDownStart;
            //    bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicDownStart);
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                    weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.MagicDownStart);
                }
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magicdownend"))
            {
            //    legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
           //     bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicDownEnd;
           //     bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicDownEnd);
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                    weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.MagicDownEnd);
                }
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magiccrouchstart"))
            {
            //    legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
            //    bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicCrouchStart;
             //   bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicCrouchStart);
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                    weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.MagicCrouchStart);
                }
                setTransitionCheckStrings(startOrEnd);
            }
            else if (startOrEnd.Equals("magiccrouchend"))
            {
             //   legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
            //    bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                currentBodyTransition = BodyTransitionIndex.MagicCrouchEnd;
            //    bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MagicCrouchEnd);
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                    weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.MagicCrouchEnd);
                }
                setTransitionCheckStrings(startOrEnd);
            }
        }

        private void drawCrouchingTransition(KeyboardState keyState, KeyboardState oldKeySate)
        {
                // this is the condition for a transition to be played. //
              //  legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
             //   bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

                
                currentLegTransition = LegTransitionIndex.Crouch;
                legsTransitions.setCurrentActiveSprite((int)LegTransitionIndex.Crouch);
               
                if (!keyState.IsKeyDown(Keys.Up))
                {
                    currentBodyTransition = BodyTransitionIndex.Crouch;
              //      bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.Crouch);
                    for (int counter = 0; counter < 3; counter++)
                    {
                        weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                        weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.Crouch);
                    }
                }
                setTransitionCheckStrings("crouchtransition");
        }
        private void drawUnCrouchingTransition(KeyboardState keyState, KeyboardState oldKeySate)
        {
           // legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;
         //   bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;

            currentLegTransition = LegTransitionIndex.UnCrouch;
            legsTransitions.setCurrentActiveSprite((int)LegTransitionIndex.UnCrouch);
            
            if (!keyState.IsKeyDown(Keys.Up))
            {
                currentBodyTransition = BodyTransitionIndex.UnCrouch;
           //     bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.UnCrouch);
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                    weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.UnCrouch);
                }
            }
            setTransitionCheckStrings("uncrouchtransition");
        }
        
        private void drawMidToUpTransition(KeyboardState keyState, KeyboardState oldKeyState)
        {
          //  bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;
           // legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

            currentBodyTransition = BodyTransitionIndex.MidToUp;
         //   bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.MidToUp);
            for (int counter = 0; counter < 3; counter++)
            {
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.MidToUp);
            }
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
                {
             //       body.setCurrentActiveSprite((int)BodySpriteIndex.IdleUp);
                    for (int counter = 0; counter < 3; counter++)
                        weaponArsenal[counter].setCurrentActiveSprite((int)BodySpriteIndex.IdleUp);
                    setCurrentActiveWeaponSprite((int)BodySpriteIndex.IdleUp);
                }
            }
        }

        private void drawUpToMidTransition(KeyboardState keyState, KeyboardState oldKeyState)
        {
            //bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;
           // legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

            for (int counter = 0; counter < 3; counter++)
            {
                weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.UpToMid);
            }
            currentBodyTransition = BodyTransitionIndex.UpToMid;
           // bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.UpToMid);
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
          //  bodyTransitions.animationCounter[bodyTransitions.currentActiveSprite] = 0;
            legsTransitions.animationCounter[legsTransitions.currentActiveSprite] = 0;

            if (!crouching && !facingUp)
            {
                currentBodyTransition = BodyTransitionIndex.Turn;
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                    weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.Turn);
                }
             //   bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.Turn);
                setTransitionCheckStrings("turntransition");

            }
            else if(crouching && !facingUp)
            {
                currentBodyTransition = BodyTransitionIndex.CrouchTurn;
            //    bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.CrouchTurn);
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                    weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.CrouchTurn);
                }
                setTransitionCheckStrings("crouchturntransition");
            }
            else if (facingUp && !crouching)
            {
                currentBodyTransition = BodyTransitionIndex.TurnUp;
             //   bodyTransitions.setCurrentActiveSprite((int)BodyTransitionIndex.TurnUp);
                for (int counter = 0; counter < 3; counter++)
                {
                    weaponTransitionArsenal[currentActiveWeapon].animationCounter[weaponTransitionArsenal[currentActiveWeapon].currentActiveSprite] = 0;
                    weaponTransitionArsenal[currentActiveWeapon].setCurrentActiveSprite((int)BodyTransitionIndex.TurnUp);
                }
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

        public static int getCurrentHeroState()
        {
            return weaponArsenal[currentActiveWeapon].getCurrentActiveSprite();
        }
        public static int getCurrentHeroTransitionState()
        {
            return weaponTransitionArsenal[currentActiveWeapon].getCurrentActiveSprite();
        }

        public void checkCrouchingHitbox()
        {
            if (crouching)
                activeHitbox = standingHitbox;
            else
                activeHitbox = standingHitbox;
        }
        public static Rectangle getHitbox()
        {
            return activeHitbox;
        }

        public static CurrentWeapon getCurrentWeapon()
        {
            return (CurrentWeapon)currentActiveWeapon;
        }

        private void setPistolHardCodedVals()
        {
            amountOfSheets = 21;

            imgPaths[(int)BodySpriteIndex.CrouchIdle] = "Sprites//Hero//PISTOL//pistol_crouchidle";
            imgPaths[(int)BodySpriteIndex.CrouchMagic] = "Sprites//Hero//PISTOL//pistol_crouchmagic";
            imgPaths[(int)BodySpriteIndex.CrouchReload] = "Sprites//Hero//PISTOL//pistol_crouchreload";
            imgPaths[(int)BodySpriteIndex.CrouchShoot] = "Sprites//Hero//PISTOL//pistol_crouchshoot";
            imgPaths[(int)BodySpriteIndex.Fall] = "Sprites//Hero//PISTOL//pistol_fall";
            imgPaths[(int)BodySpriteIndex.GetHurt] = "Sprites//Hero//PISTOL//pistol_gethurt";
            imgPaths[(int)BodySpriteIndex.HangIdle] = "Sprites//Hero//PISTOL//pistol_hangidle";
            imgPaths[(int)BodySpriteIndex.Idle] = "Sprites//Hero//PISTOL//pistol_idle";
            imgPaths[(int)BodySpriteIndex.IdleDown] = "Sprites//Hero//PISTOL//pistol_idledown";
            imgPaths[(int)BodySpriteIndex.IdleUp] = "Sprites//Hero//PISTOL//pistol_idleup";
            imgPaths[(int)BodySpriteIndex.Jump] = "Sprites//Hero//PISTOL//pistol_jump";
            imgPaths[(int)BodySpriteIndex.Magic] = "Sprites//Hero//PISTOL//pistol_magic";
            imgPaths[(int)BodySpriteIndex.MagicDown] = "Sprites//Hero//PISTOL//pistol_magicdown";
            imgPaths[(int)BodySpriteIndex.MagicUp] = "Sprites//Hero//PISTOL//pistol_magicup";
            imgPaths[(int)BodySpriteIndex.PullTowards] = "Sprites//Hero//PISTOL//pistol_pulltowards";
            imgPaths[(int)BodySpriteIndex.PushAway] = "Sprites//Hero//PISTOL//pistol_pushaway";
            imgPaths[(int)BodySpriteIndex.Reload] = "Sprites//Hero//PISTOL//pistol_reload"; // added reload //
            imgPaths[(int)BodySpriteIndex.ReloadUp] = "Sprites//Hero//PISTOL//pistol_reloadup";
            imgPaths[(int)BodySpriteIndex.Shoot] = "Sprites//Hero//PISTOL//pistol_shoot";
            imgPaths[(int)BodySpriteIndex.ShootDown] = "Sprites//Hero//PISTOL//pistol_shootdown";
            imgPaths[(int)BodySpriteIndex.ShootUp] = "Sprites//Hero//PISTOL//pistol_shootup";

            frameAmounts[(int)BodySpriteIndex.CrouchIdle] = 25;
            frameAmounts[(int)BodySpriteIndex.CrouchMagic] = 5;
            frameAmounts[(int)BodySpriteIndex.CrouchReload] = 18;
            frameAmounts[(int)BodySpriteIndex.CrouchShoot] = 11;
            frameAmounts[(int)BodySpriteIndex.Fall] = 1;
            frameAmounts[(int)BodySpriteIndex.GetHurt] = 1;
            frameAmounts[(int)BodySpriteIndex.HangIdle] = 27;
            frameAmounts[(int)BodySpriteIndex.Idle] = 25;
            frameAmounts[(int)BodySpriteIndex.IdleDown] = 1;
            frameAmounts[(int)BodySpriteIndex.IdleUp] = 25;
            frameAmounts[(int)BodySpriteIndex.Jump] = 1;
            frameAmounts[(int)BodySpriteIndex.Magic] = 5;
            frameAmounts[(int)BodySpriteIndex.MagicDown] = 5;
            frameAmounts[(int)BodySpriteIndex.MagicUp] = 5;
            frameAmounts[(int)BodySpriteIndex.PullTowards] = 6;
            frameAmounts[(int)BodySpriteIndex.PushAway] = 6;
            frameAmounts[(int)BodySpriteIndex.Reload] = 18;
            frameAmounts[(int)BodySpriteIndex.ReloadUp] = 18;
            frameAmounts[(int)BodySpriteIndex.Shoot] = 11;
            frameAmounts[(int)BodySpriteIndex.ShootDown] = 11;
            frameAmounts[(int)BodySpriteIndex.ShootUp] = 11;

            for (int counter = 0; counter < amountOfSheets; counter++)
            {
                frameRects[counter] = new Rectangle(0, 0, 186, 208);
                spriteDelayTimes[counter] = MILLISECOND_DELAY;
            }

            startingPos = new Vector2(bodyStartPos.X, bodyStartPos.Y);
            
        }
        private void setRifleHardCodedVals()
        {
            amountOfSheets = 21;

            imgPaths[(int)BodySpriteIndex.CrouchIdle] = "Sprites//Hero//MACHINE GUN//machine_gun_crouchidle";
            imgPaths[(int)BodySpriteIndex.CrouchMagic] = "Sprites//Hero//MACHINE GUN//machine_gun_crouchmagic";
            imgPaths[(int)BodySpriteIndex.CrouchReload] = "Sprites//Hero//MACHINE GUN//machine_gun_crouchreload";
            imgPaths[(int)BodySpriteIndex.CrouchShoot] = "Sprites//Hero//MACHINE GUN//machine_gun_crouchshoot";
            imgPaths[(int)BodySpriteIndex.Fall] = "Sprites//Hero//MACHINE GUN//machine_gun_fall";
            imgPaths[(int)BodySpriteIndex.GetHurt] = "Sprites//Hero//MACHINE GUN//machine_gun_gethurt";
            imgPaths[(int)BodySpriteIndex.HangIdle] = "Sprites//Hero//MACHINE GUN//machine_gun_hangidle";
            imgPaths[(int)BodySpriteIndex.Idle] = "Sprites//Hero//MACHINE GUN//machine_gun_idle";
            imgPaths[(int)BodySpriteIndex.IdleDown] = "Sprites//Hero//MACHINE GUN//machine_gun_idledown";
            imgPaths[(int)BodySpriteIndex.IdleUp] = "Sprites//Hero//MACHINE GUN//machine_gun_idleup";
            imgPaths[(int)BodySpriteIndex.Jump] = "Sprites//Hero//MACHINE GUN//machine_gun_jump";
            imgPaths[(int)BodySpriteIndex.Magic] = "Sprites//Hero//MACHINE GUN//machine_gun_magic";
            imgPaths[(int)BodySpriteIndex.MagicDown] = "Sprites//Hero//MACHINE GUN//machine_gun_magicdown";
            imgPaths[(int)BodySpriteIndex.MagicUp] = "Sprites//Hero//MACHINE GUN//machine_gun_magicup";
            imgPaths[(int)BodySpriteIndex.PullTowards] = "Sprites//Hero//MACHINE GUN//machine_gun_pulltowards";
            imgPaths[(int)BodySpriteIndex.PushAway] = "Sprites//Hero//MACHINE GUN//machine_gun_pushaway";
            imgPaths[(int)BodySpriteIndex.Reload] = "Sprites//Hero//MACHINE GUN//machine_gun_reload"; // added reload //
            imgPaths[(int)BodySpriteIndex.ReloadUp] = "Sprites//Hero//MACHINE GUN//machine_gun_reloadup";
            imgPaths[(int)BodySpriteIndex.Shoot] = "Sprites//Hero//MACHINE GUN//machine_gun_shoot";
            imgPaths[(int)BodySpriteIndex.ShootDown] = "Sprites//Hero//MACHINE GUN//machine_gun_shootdown";
            imgPaths[(int)BodySpriteIndex.ShootUp] = "Sprites//Hero//MACHINE GUN//machine_gun_shootup";

            frameAmounts[(int)BodySpriteIndex.CrouchIdle] = 25;
            frameAmounts[(int)BodySpriteIndex.CrouchMagic] = 5;
            frameAmounts[(int)BodySpriteIndex.CrouchReload] = 19;
            frameAmounts[(int)BodySpriteIndex.CrouchShoot] = 4;
            frameAmounts[(int)BodySpriteIndex.Fall] = 1;
            frameAmounts[(int)BodySpriteIndex.GetHurt] = 1;
            frameAmounts[(int)BodySpriteIndex.HangIdle] = 25;
            frameAmounts[(int)BodySpriteIndex.Idle] = 25;
            frameAmounts[(int)BodySpriteIndex.IdleDown] = 1;
            frameAmounts[(int)BodySpriteIndex.IdleUp] = 25;
            frameAmounts[(int)BodySpriteIndex.Jump] = 1;
            frameAmounts[(int)BodySpriteIndex.Magic] = 5;
            frameAmounts[(int)BodySpriteIndex.MagicDown] = 5;
            frameAmounts[(int)BodySpriteIndex.MagicUp] = 5;
            frameAmounts[(int)BodySpriteIndex.PullTowards] = 6;
            frameAmounts[(int)BodySpriteIndex.PushAway] = 6;
            frameAmounts[(int)BodySpriteIndex.Reload] = 19;
            frameAmounts[(int)BodySpriteIndex.ReloadUp] = 19;
            frameAmounts[(int)BodySpriteIndex.Shoot] = 4;
            frameAmounts[(int)BodySpriteIndex.ShootDown] = 4;
            frameAmounts[(int)BodySpriteIndex.ShootUp] = 4;

            for (int counter = 0; counter < amountOfSheets; counter++)
            {
                frameRects[counter] = new Rectangle(0, 0, 186, 208);
                spriteDelayTimes[counter] = MILLISECOND_DELAY;
            }

            startingPos = new Vector2(bodyStartPos.X, bodyStartPos.Y);
            
        }
        private void setRocketHardCodedVals()
        {
            amountOfSheets = 21;

            imgPaths[(int)BodySpriteIndex.CrouchIdle] = "Sprites//Hero//ROCKET//rckt_crouchidle";
            imgPaths[(int)BodySpriteIndex.CrouchMagic] = "Sprites//Hero//ROCKET//rckt_crouchmagic";
            imgPaths[(int)BodySpriteIndex.CrouchReload] = "Sprites//Hero//ROCKET//rckt_crouchreload";
            imgPaths[(int)BodySpriteIndex.CrouchShoot] = "Sprites//Hero//ROCKET//rckt_crouchshoot";
            imgPaths[(int)BodySpriteIndex.Fall] = "Sprites//Hero//ROCKET//rckt_fall";
            imgPaths[(int)BodySpriteIndex.GetHurt] = "Sprites//Hero//ROCKET//rckt_gethurt";
            imgPaths[(int)BodySpriteIndex.HangIdle] = "Sprites//Hero//ROCKET//rckt_hangidle";
            imgPaths[(int)BodySpriteIndex.Idle] = "Sprites//Hero//ROCKET//rckt_idle";
            imgPaths[(int)BodySpriteIndex.IdleDown] = "Sprites//Hero//ROCKET//rckt_idledown";
            imgPaths[(int)BodySpriteIndex.IdleUp] = "Sprites//Hero//ROCKET//rckt_idleup";
            imgPaths[(int)BodySpriteIndex.Jump] = "Sprites//Hero//ROCKET//rckt_jump";
            imgPaths[(int)BodySpriteIndex.Magic] = "Sprites//Hero//ROCKET//rckt_magic";
            imgPaths[(int)BodySpriteIndex.MagicDown] = "Sprites//Hero//ROCKET//rckt_magicdown";
            imgPaths[(int)BodySpriteIndex.MagicUp] = "Sprites//Hero//ROCKET//rckt_magicup";
            imgPaths[(int)BodySpriteIndex.PullTowards] = "Sprites//Hero//ROCKET//rckt_pulltowards";
            imgPaths[(int)BodySpriteIndex.PushAway] = "Sprites//Hero//ROCKET//rckt_pushaway";
            imgPaths[(int)BodySpriteIndex.Reload] = "Sprites//Hero//ROCKET//rckt_reload"; // added reload //
            imgPaths[(int)BodySpriteIndex.ReloadUp] = "Sprites//Hero//ROCKET//rckt_reloadup";
            imgPaths[(int)BodySpriteIndex.Shoot] = "Sprites//Hero//ROCKET//rckt_shoot";
            imgPaths[(int)BodySpriteIndex.ShootDown] = "Sprites//Hero//ROCKET//rckt_shootdown";
            imgPaths[(int)BodySpriteIndex.ShootUp] = "Sprites//Hero//ROCKET//rckt_shootup";

            frameAmounts[(int)BodySpriteIndex.CrouchIdle] = 25;
            frameAmounts[(int)BodySpriteIndex.CrouchMagic] = 5;
            frameAmounts[(int)BodySpriteIndex.CrouchReload] = 19;
            frameAmounts[(int)BodySpriteIndex.CrouchShoot] = 17;
            frameAmounts[(int)BodySpriteIndex.Fall] = 1;
            frameAmounts[(int)BodySpriteIndex.GetHurt] = 1;
            frameAmounts[(int)BodySpriteIndex.HangIdle] = 25;
            frameAmounts[(int)BodySpriteIndex.Idle] = 25;
            frameAmounts[(int)BodySpriteIndex.IdleDown] = 1;
            frameAmounts[(int)BodySpriteIndex.IdleUp] = 25;
            frameAmounts[(int)BodySpriteIndex.Jump] = 1;
            frameAmounts[(int)BodySpriteIndex.Magic] = 5;
            frameAmounts[(int)BodySpriteIndex.MagicDown] = 5;
            frameAmounts[(int)BodySpriteIndex.MagicUp] = 5;
            frameAmounts[(int)BodySpriteIndex.PullTowards] = 6;
            frameAmounts[(int)BodySpriteIndex.PushAway] = 6;
            frameAmounts[(int)BodySpriteIndex.Reload] = 19;
            frameAmounts[(int)BodySpriteIndex.ReloadUp] = 19;
            frameAmounts[(int)BodySpriteIndex.Shoot] = 17;
            frameAmounts[(int)BodySpriteIndex.ShootDown] = 17;
            frameAmounts[(int)BodySpriteIndex.ShootUp] = 17;

            for (int counter = 0; counter < amountOfSheets; counter++)
            {
                frameRects[counter] = new Rectangle(0, 0, 186, 208);
                spriteDelayTimes[counter] = MILLISECOND_DELAY;
            }

            startingPos = new Vector2(bodyStartPos.X, bodyStartPos.Y);
            
        }

        private void setCurrentActiveWeaponSprite(int newState)
        {
            for (int counter = 0; counter < 3; counter++)
            {
                weaponArsenal[counter].setCurrentActiveSprite(newState);
            }
        }
        private void setCurrentActiveWeaponTransitionSprite(int newState)
        {
            for (int counter = 0; counter < 3; counter++)
            {
                weaponTransitionArsenal[counter].setCurrentActiveSprite(newState);
            }
        }

        private void setPistolTransitionHardCodedVals()
        {
            amountOfSheets = 39; // this is for the TOTAL amount of sheets that wil be loaded (this number will obviously increase over time as different animations will be needed) //

            imgPaths[(int)BodyTransitionIndex.Crouch] = "Sprites//Hero//PISTOL//Transitions//pistol_crouch";
            imgPaths[(int)BodyTransitionIndex.CrouchHolster] = "Sprites//Hero//PISTOL//Transitions//pistol_crouchholster";
            imgPaths[(int)BodyTransitionIndex.CrouchReload] = "Sprites//Hero//PISTOL//Transitions//pistol_crouchreload";
            imgPaths[(int)BodyTransitionIndex.CrouchTurn] = "Sprites//Hero//PISTOL//Transitions//pistol_crouchturn";
            imgPaths[(int)BodyTransitionIndex.Fall] = "Sprites//Hero//PISTOL//Transitions//pistol_fall";
            imgPaths[(int)BodyTransitionIndex.GetHurt] = "Sprites//Hero//PISTOL//Transitions//pistol_gethurt";
            imgPaths[(int)BodyTransitionIndex.GrabLedge] = "Sprites//Hero//PISTOL//Transitions//pistol_grabledge";
            imgPaths[(int)BodyTransitionIndex.Hoist] = "Sprites//Hero//PISTOL//Transitions//pistol_hoist";
            imgPaths[(int)BodyTransitionIndex.Holster] = "Sprites//Hero//PISTOL//Transitions//pistol_holster";
            imgPaths[(int)BodyTransitionIndex.HolsterDown] = "Sprites//Hero//PISTOL//Transitions//pistol_holsterdown";
            imgPaths[(int)BodyTransitionIndex.HolsterUp] = "Sprites//Hero//PISTOL//Transitions//pistol_holsterup";
            imgPaths[(int)BodyTransitionIndex.Jump] = "Sprites//Hero//PISTOL//Transitions//pistol_jump";
            imgPaths[(int)BodyTransitionIndex.LetGo] = "Sprites//Hero//PISTOL//Transitions//pistol_letgo";
            imgPaths[(int)BodyTransitionIndex.LowToMid] = "Sprites//Hero//PISTOL//Transitions//pistol_lowtomid";
            imgPaths[(int)BodyTransitionIndex.MagicCrouchEnd] = "Sprites//Hero//PISTOL//Transitions//pistol_magiccrouchend";
            imgPaths[(int)BodyTransitionIndex.MagicCrouchStart] = "Sprites//Hero//PISTOL//Transitions//pistol_magiccrouchstart";
            imgPaths[(int)BodyTransitionIndex.MagicDownEnd] = "Sprites//Hero//PISTOL//Transitions//pistol_magicdownend";
            imgPaths[(int)BodyTransitionIndex.MagicDownStart] = "Sprites//Hero//PISTOL//Transitions//pistol_magicdownstart";
            imgPaths[(int)BodyTransitionIndex.MagicEnd] = "Sprites//Hero//PISTOL//Transitions//pistol_magicend";
            imgPaths[(int)BodyTransitionIndex.MagicStart] = "Sprites//Hero//PISTOL//Transitions//pistol_magicstart";
            imgPaths[(int)BodyTransitionIndex.MagicUpEnd] = "Sprites//Hero//PISTOL//Transitions//pistol_magicupend";
            imgPaths[(int)BodyTransitionIndex.MagicUpStart] = "Sprites//Hero//PISTOL//Transitions//pistol_magicupstart";
            imgPaths[(int)BodyTransitionIndex.MidToLow] = "Sprites//Hero//PISTOL//Transitions//pistol_midtolow";
            imgPaths[(int)BodyTransitionIndex.MidToUp] = "Sprites//Hero//PISTOL//Transitions//pistol_midtoup";
            imgPaths[(int)BodyTransitionIndex.Reload] = "Sprites//Hero//PISTOL//Transitions//pistol_reload";
            imgPaths[(int)BodyTransitionIndex.ReloadUp] = "Sprites//Hero//PISTOL//Transitions//pistol_reloadup";
            imgPaths[(int)BodyTransitionIndex.ShootCrouchEnd] = "Sprites//Hero//PISTOL//Transitions//pistol_shootcrouchend";
            imgPaths[(int)BodyTransitionIndex.ShootCrouchStart] = "Sprites//Hero//PISTOL//Transitions//pistol_shootcrouchstart";
            imgPaths[(int)BodyTransitionIndex.ShootDownEnd] = "Sprites//Hero//PISTOL//Transitions//pistol_shootdownend";
            imgPaths[(int)BodyTransitionIndex.ShootDownStart] = "Sprites//Hero//PISTOL//Transitions//pistol_shootdownstart";
            imgPaths[(int)BodyTransitionIndex.ShootEnd] = "Sprites//Hero//PISTOL//Transitions//pistol_shootend";
            imgPaths[(int)BodyTransitionIndex.ShootStart] = "Sprites//Hero//PISTOL//Transitions//pistol_shootstart";
            imgPaths[(int)BodyTransitionIndex.ShootUpEnd] = "Sprites//Hero//PISTOL//Transitions//pistol_shootupend";
            imgPaths[(int)BodyTransitionIndex.ShootUpStart] = "Sprites//Hero//PISTOL//Transitions//pistol_shootupstart";
            imgPaths[(int)BodyTransitionIndex.Turn] = "Sprites//Hero//PISTOL//Transitions//pistol_turn";
            imgPaths[(int)BodyTransitionIndex.TurnDown] = "Sprites//Hero//PISTOL//Transitions//pistol_turndown";
            imgPaths[(int)BodyTransitionIndex.TurnUp] = "Sprites//Hero//PISTOL//Transitions//pistol_turnup";
            imgPaths[(int)BodyTransitionIndex.UnCrouch] = "Sprites//Hero//PISTOL//Transitions//pistol_uncrouch";
            imgPaths[(int)BodyTransitionIndex.UpToMid] = "Sprites//Hero//PISTOL//Transitions//pistol_uptomid";

            frameAmounts[(int)BodyTransitionIndex.Crouch] = 6;
            frameAmounts[(int)BodyTransitionIndex.CrouchHolster] = 8;
            frameAmounts[(int)BodyTransitionIndex.CrouchReload] = 18;
            frameAmounts[(int)BodyTransitionIndex.CrouchTurn] = 4;
            frameAmounts[(int)BodyTransitionIndex.Fall] = 7;
            frameAmounts[(int)BodyTransitionIndex.GetHurt] = 6;
            frameAmounts[(int)BodyTransitionIndex.GrabLedge] = 9;
            frameAmounts[(int)BodyTransitionIndex.Hoist] = 14;
            frameAmounts[(int)BodyTransitionIndex.Holster] = 8;
            frameAmounts[(int)BodyTransitionIndex.HolsterDown] = 8;
            frameAmounts[(int)BodyTransitionIndex.HolsterUp] = 8;
            frameAmounts[(int)BodyTransitionIndex.Jump] = 7;
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
            frameAmounts[(int)BodyTransitionIndex.Reload] = 18;
            frameAmounts[(int)BodyTransitionIndex.ReloadUp] = 18;
            frameAmounts[(int)BodyTransitionIndex.ShootCrouchEnd] = 1;
            frameAmounts[(int)BodyTransitionIndex.ShootCrouchStart] = 11;
            frameAmounts[(int)BodyTransitionIndex.ShootDownEnd] = 1;
            frameAmounts[(int)BodyTransitionIndex.ShootDownStart] = 11; // BREAK THROUGH //
            frameAmounts[(int)BodyTransitionIndex.ShootEnd] = 1;
            frameAmounts[(int)BodyTransitionIndex.ShootStart] = 11;
            frameAmounts[(int)BodyTransitionIndex.ShootUpEnd] = 1; 
            frameAmounts[(int)BodyTransitionIndex.ShootUpStart] = 11;
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
        private void setRifleTransitionHardCodedVals()
        {
            amountOfSheets = 39; // this is for the TOTAL amount of sheets that wil be loaded (this number will obviously increase over time as different animations will be needed) //

            imgPaths[(int)BodyTransitionIndex.Crouch] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_crouch";
            imgPaths[(int)BodyTransitionIndex.CrouchHolster] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_crouchholster";
            imgPaths[(int)BodyTransitionIndex.CrouchReload] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_crouchreload";
            imgPaths[(int)BodyTransitionIndex.CrouchTurn] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_crouchturn";
            imgPaths[(int)BodyTransitionIndex.Fall] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_fall";
            imgPaths[(int)BodyTransitionIndex.GetHurt] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_gethurt";
            imgPaths[(int)BodyTransitionIndex.GrabLedge] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_grabledge";
            imgPaths[(int)BodyTransitionIndex.Hoist] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_hoist";
            imgPaths[(int)BodyTransitionIndex.Holster] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_holster";
            imgPaths[(int)BodyTransitionIndex.HolsterDown] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_holsterdown";
            imgPaths[(int)BodyTransitionIndex.HolsterUp] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_holsterup";
            imgPaths[(int)BodyTransitionIndex.Jump] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_jump";
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
            imgPaths[(int)BodyTransitionIndex.Reload] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_reload";
            imgPaths[(int)BodyTransitionIndex.ReloadUp] = "Sprites//Hero//MACHINE GUN//Transitions//machine_gun_reloadup";
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
            frameAmounts[(int)BodyTransitionIndex.CrouchReload] = 19;
            frameAmounts[(int)BodyTransitionIndex.CrouchTurn] = 4;
            frameAmounts[(int)BodyTransitionIndex.Fall] = 7;
            frameAmounts[(int)BodyTransitionIndex.GetHurt] = 6;
            frameAmounts[(int)BodyTransitionIndex.GrabLedge] = 9;
            frameAmounts[(int)BodyTransitionIndex.Hoist] = 14;
            frameAmounts[(int)BodyTransitionIndex.Holster] = 8;
            frameAmounts[(int)BodyTransitionIndex.HolsterDown] = 8;
            frameAmounts[(int)BodyTransitionIndex.HolsterUp] = 8;
            frameAmounts[(int)BodyTransitionIndex.Jump] = 7;
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
            frameAmounts[(int)BodyTransitionIndex.Reload] = 19;
            frameAmounts[(int)BodyTransitionIndex.ReloadUp] = 19;
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
        private void setRocketTransitionHardCodedVals()
        {
            amountOfSheets = 39; // this is for the TOTAL amount of sheets that wil be loaded (this number will obviously increase over time as different animations will be needed) //

            imgPaths[(int)BodyTransitionIndex.Crouch] = "Sprites//Hero//ROCKET//Transitions//rckt_crouch";
            imgPaths[(int)BodyTransitionIndex.CrouchHolster] = "Sprites//Hero//ROCKET//Transitions//rckt_crouchholster";
            imgPaths[(int)BodyTransitionIndex.CrouchReload] = "Sprites//Hero//ROCKET//Transitions//rckt_crouchreload";
            imgPaths[(int)BodyTransitionIndex.CrouchTurn] = "Sprites//Hero//ROCKET//Transitions//rckt_crouchturn";
            imgPaths[(int)BodyTransitionIndex.Fall] = "Sprites//Hero//ROCKET//Transitions//rckt_fall";
            imgPaths[(int)BodyTransitionIndex.GetHurt] = "Sprites//Hero//ROCKET//Transitions//rckt_gethurt";
            imgPaths[(int)BodyTransitionIndex.GrabLedge] = "Sprites//Hero//ROCKET//Transitions//rckt_grabledge";
            imgPaths[(int)BodyTransitionIndex.Hoist] = "Sprites//Hero//ROCKET//Transitions//rckt_hoist";
            imgPaths[(int)BodyTransitionIndex.Holster] = "Sprites//Hero//ROCKET//Transitions//rckt_holster";
            imgPaths[(int)BodyTransitionIndex.HolsterDown] = "Sprites//Hero//ROCKET//Transitions//rckt_holsterdown";
            imgPaths[(int)BodyTransitionIndex.HolsterUp] = "Sprites//Hero//ROCKET//Transitions//rckt_holsterup";
            imgPaths[(int)BodyTransitionIndex.Jump] = "Sprites//Hero//ROCKET//Transitions//rckt_jump";
            imgPaths[(int)BodyTransitionIndex.LetGo] = "Sprites//Hero//ROCKET//Transitions//rckt_letgo";
            imgPaths[(int)BodyTransitionIndex.LowToMid] = "Sprites//Hero//ROCKET//Transitions//rckt_lowtomid";
            imgPaths[(int)BodyTransitionIndex.MagicCrouchEnd] = "Sprites//Hero//ROCKET//Transitions//rckt_magiccrouchend";
            imgPaths[(int)BodyTransitionIndex.MagicCrouchStart] = "Sprites//Hero//ROCKET//Transitions//rckt_magiccrouchstart";
            imgPaths[(int)BodyTransitionIndex.MagicDownEnd] = "Sprites//Hero//ROCKET//Transitions//rckt_magicdownend";
            imgPaths[(int)BodyTransitionIndex.MagicDownStart] = "Sprites//Hero//ROCKET//Transitions//rckt_magicdownstart";
            imgPaths[(int)BodyTransitionIndex.MagicEnd] = "Sprites//Hero//ROCKET//Transitions//rckt_magicend";
            imgPaths[(int)BodyTransitionIndex.MagicStart] = "Sprites//Hero//ROCKET//Transitions//rckt_magicstart";
            imgPaths[(int)BodyTransitionIndex.MagicUpEnd] = "Sprites//Hero//ROCKET//Transitions//rckt_magicupend";
            imgPaths[(int)BodyTransitionIndex.MagicUpStart] = "Sprites//Hero//ROCKET//Transitions//rckt_magicupstart";
            imgPaths[(int)BodyTransitionIndex.MidToLow] = "Sprites//Hero//ROCKET//Transitions//rckt_midtolow";
            imgPaths[(int)BodyTransitionIndex.MidToUp] = "Sprites//Hero//ROCKET//Transitions//rckt_midtoup";
            imgPaths[(int)BodyTransitionIndex.Reload] = "Sprites//Hero//ROCKET//Transitions//rckt_reload";
            imgPaths[(int)BodyTransitionIndex.ReloadUp] = "Sprites//Hero//ROCKET//Transitions//rckt_reloadup";
            imgPaths[(int)BodyTransitionIndex.ShootCrouchEnd] = "Sprites//Hero//ROCKET//Transitions//rckt_shootcrouchend";
            imgPaths[(int)BodyTransitionIndex.ShootCrouchStart] = "Sprites//Hero//ROCKET//Transitions//rckt_shootcrouchstart";
            imgPaths[(int)BodyTransitionIndex.ShootDownEnd] = "Sprites//Hero//ROCKET//Transitions//rckt_shootdownend";
            imgPaths[(int)BodyTransitionIndex.ShootDownStart] = "Sprites//Hero//ROCKET//Transitions//rckt_shootdownstart";
            imgPaths[(int)BodyTransitionIndex.ShootEnd] = "Sprites//Hero//ROCKET//Transitions//rckt_shootend";
            imgPaths[(int)BodyTransitionIndex.ShootStart] = "Sprites//Hero//ROCKET//Transitions//rckt_shootstart";
            imgPaths[(int)BodyTransitionIndex.ShootUpEnd] = "Sprites//Hero//ROCKET//Transitions//rckt_shootupend";
            imgPaths[(int)BodyTransitionIndex.ShootUpStart] = "Sprites//Hero//ROCKET//Transitions//rckt_shootupstart";
            imgPaths[(int)BodyTransitionIndex.Turn] = "Sprites//Hero//ROCKET//Transitions//rckt_turn";
            imgPaths[(int)BodyTransitionIndex.TurnDown] = "Sprites//Hero//ROCKET//Transitions//rckt_turndown";
            imgPaths[(int)BodyTransitionIndex.TurnUp] = "Sprites//Hero//ROCKET//Transitions//rckt_turnup";
            imgPaths[(int)BodyTransitionIndex.UnCrouch] = "Sprites//Hero//ROCKET//Transitions//rckt_uncrouch";
            imgPaths[(int)BodyTransitionIndex.UpToMid] = "Sprites//Hero//ROCKET//Transitions//rckt_uptomid";

            frameAmounts[(int)BodyTransitionIndex.Crouch] = 6;
            frameAmounts[(int)BodyTransitionIndex.CrouchHolster] = 8;
            frameAmounts[(int)BodyTransitionIndex.CrouchReload] = 25;
            frameAmounts[(int)BodyTransitionIndex.CrouchTurn] = 4;
            frameAmounts[(int)BodyTransitionIndex.Fall] = 7;
            frameAmounts[(int)BodyTransitionIndex.GetHurt] = 6;
            frameAmounts[(int)BodyTransitionIndex.GrabLedge] = 9;
            frameAmounts[(int)BodyTransitionIndex.Hoist] = 14;
            frameAmounts[(int)BodyTransitionIndex.Holster] = 8;
            frameAmounts[(int)BodyTransitionIndex.HolsterDown] = 8;
            frameAmounts[(int)BodyTransitionIndex.HolsterUp] = 8;
            frameAmounts[(int)BodyTransitionIndex.Jump] = 7;
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
            frameAmounts[(int)BodyTransitionIndex.Reload] = 25;
            frameAmounts[(int)BodyTransitionIndex.ReloadUp] = 24;
            frameAmounts[(int)BodyTransitionIndex.ShootCrouchEnd] = 1;
            frameAmounts[(int)BodyTransitionIndex.ShootCrouchStart] = 17;
            frameAmounts[(int)BodyTransitionIndex.ShootDownEnd] = 1;
            frameAmounts[(int)BodyTransitionIndex.ShootDownStart] = 17;
            frameAmounts[(int)BodyTransitionIndex.ShootEnd] = 1;
            frameAmounts[(int)BodyTransitionIndex.ShootStart] = 17;
            frameAmounts[(int)BodyTransitionIndex.ShootUpEnd] = 1;
            frameAmounts[(int)BodyTransitionIndex.ShootUpStart] = 17;
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
            frameAmounts[(int)LegSpriteIndex.Fall] = 1;
            frameAmounts[(int)LegSpriteIndex.GetHurt] = 1;
            frameAmounts[(int)LegSpriteIndex.Idle] = 25;
            frameAmounts[(int)LegSpriteIndex.Jump] = 1;
            frameAmounts[(int)LegSpriteIndex.Running] = 15;
            frameAmounts[(int)LegSpriteIndex.CrouchIdle] = 1;

            for (int counter = 0; counter < amountOfSheets; counter++)
            {
                frameRects[counter] = new Rectangle(0, 0, 111, 91);
                spriteDelayTimes[counter] = MILLISECOND_DELAY;
            }

            startingPos = new Vector2(legsStartPos.X, legsStartPos.Y);
          
        }
        
        private void setLegsTransitionHardCodedValues()
        {
            amountOfSheets = 7; // total number of sheets to be loaded //

            imgPaths[(int)LegTransitionIndex.Crouch] = "Sprites//Hero//LEGS//Transitions//crouch";
            imgPaths[(int)LegTransitionIndex.Fall] = "Sprites//Hero//LEGS//Transitions//fall";
            imgPaths[(int)LegTransitionIndex.GetHurt] = "Sprites//Hero//LEGS//Transitions//gethurt";
            imgPaths[(int)LegTransitionIndex.Jump] = "Sprites//Hero//LEGS//Transitions//jump";
            imgPaths[(int)LegTransitionIndex.TwistJump] = "Sprites//Hero//LEGS//Transitions//twistjump";
            imgPaths[(int)LegTransitionIndex.UnCrouch] = "Sprites//Hero//LEGS//Transitions//uncrouch";

            frameAmounts[(int)LegTransitionIndex.Crouch]  = 5;
            frameAmounts[(int)LegTransitionIndex.Fall] = 2;
            frameAmounts[(int)LegTransitionIndex.GetHurt] = 6;
            frameAmounts[(int)LegTransitionIndex.Jump] = 9;
            frameAmounts[(int)LegTransitionIndex.TwistJump] = 9;
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
            frameAmounts = new int[40]; 
            frameRects = new Rectangle[40];
            imgPaths = new string[40];
            spriteDelayTimes = new int[40];
        }

        private void loadArsenal(ContentManager content)
        {
            
            // Set Body Parts
            setPistolHardCodedVals();
            weaponArsenal[(int)CurrentWeapon.Pistol] = new Weapon(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);
            
            setRifleHardCodedVals();
            weaponArsenal[(int)CurrentWeapon.Rifle] = new Weapon(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);

            setRocketHardCodedVals();
            weaponArsenal[(int)CurrentWeapon.Rocket] = new Weapon(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);

            // Set Transitions //
            setPistolTransitionHardCodedVals();
            weaponTransitionArsenal[(int)CurrentWeapon.Pistol] = new WeaponTransition(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);
            
            setRifleTransitionHardCodedVals();
            weaponTransitionArsenal[(int)CurrentWeapon.Rifle] = new WeaponTransition(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);
            
            setRocketTransitionHardCodedVals();
            weaponTransitionArsenal[(int)CurrentWeapon.Rocket] = new WeaponTransition(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);

        }

        public static Rectangle getHeroHitbox()
        {
            return newHitbox;
        }

        public void passPlatform(Platform plat, Platform obs)
        {
            floor = plat;
            obstacle = obs;
        } // for debugging collision & jumping //
    }
}
