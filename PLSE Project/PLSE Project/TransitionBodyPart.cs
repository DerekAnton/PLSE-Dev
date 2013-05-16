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
    class TransitionBodyPart : BodyPart
    {
        public TransitionBodyPart(ContentManager content, int amountOfSheets, int[] frameAmount, Rectangle[] sourceRect, string[] imgPath, Vector2 startingPos, int[] frameDelayTimes)
            : base(content, amountOfSheets, frameAmount, sourceRect, imgPath, startingPos, frameDelayTimes)
        { }
        public void animateUntilEndFrame(GameTime gameTime, ContentManager content)
        {
            frameLimiter += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (frameLimiter >= lengthOfTimePerFrame[currentActiveSprite])
            {
                frameLimiter = 0;
                sourceRect[currentActiveSprite].X = (animationCounter[currentActiveSprite] % 10) * sourceRect[currentActiveSprite].Width;
                sourceRect[currentActiveSprite].Y = (animationCounter[currentActiveSprite] / 10) * sourceRect[currentActiveSprite].Height;
                animationCounter[currentActiveSprite]++;

                addBullets(content); // ONLY FOR PISTOL AND RCKT // //MACHINE GUN ADD BULLET WILL BE IN BODYPART ANIMATION LOGIC//

                if (animationCounter[currentActiveSprite] >= frameAmounts[currentActiveSprite])
                {
                    animationCounter[currentActiveSprite] = 0;
                    Hero.setBodyNull();                       
                    Hero.setLegNull();
                }
            }
        }

        private void addBullets(ContentManager content)
        {
            if (Hero.currentActiveWeapon == (int)CurrentWeapon.Pistol && checkIfFiringTransition() && animationCounter[currentActiveSprite] == 2)
                ProjectileManager.addBullet("pistol", content);
            else if (Hero.currentActiveWeapon == (int)CurrentWeapon.Rocket && checkIfFiringTransition() && animationCounter[currentActiveSprite] == 4)
                ProjectileManager.addBullet("rckt", content);
        }

        private bool checkIfFiringTransition()
        {
            if (currentActiveSprite == (int)BodyTransitionIndex.ShootStart)
                return true;
            else if (currentActiveSprite == (int)BodyTransitionIndex.ShootUpStart)
                return true;
            else if (currentActiveSprite == (int)BodyTransitionIndex.ShootDownStart)
                return true;
            else if (currentActiveSprite == (int)BodyTransitionIndex.ShootCrouchStart)
                return true;
            else
                return false;
        }
    }
}
