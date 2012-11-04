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
    class TransitionBodyPart : BodyPart
    {
        public TransitionBodyPart(ContentManager content, int amountOfSheets, int[] frameAmount, Rectangle[] sourceRect, string[] imgPath, Vector2 startingPos, int[] frameDelayTimes)
            : base(content, amountOfSheets, frameAmount, sourceRect, imgPath, startingPos, frameDelayTimes)
        {

        }
        public void animateUntilEndFrame(GameTime gameTime)
        {
            frameLimiter += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (frameLimiter >= lengthOfTimePerFrame[currentActiveSprite])
            {
                
                frameLimiter = 0;
                if (printme)
                    Console.WriteLine(animationCounter[currentActiveSprite]);


                sourceRect[currentActiveSprite].X = (animationCounter[currentActiveSprite] % 10) * sourceRect[currentActiveSprite].Width;
                sourceRect[currentActiveSprite].Y = (animationCounter[currentActiveSprite] / 10) * sourceRect[currentActiveSprite].Height;
                animationCounter[currentActiveSprite]++;

                if (animationCounter[currentActiveSprite] >= frameAmounts[currentActiveSprite])
                {
                    animationCounter[currentActiveSprite] = 0;
                    Hero.setBodyNull();                       
                    Hero.setLegNull();
                }
                
                /*
                if ((sourceRect[currentActiveSprite].X + sourceRect[currentActiveSprite].Width) < sheetWidths[currentActiveSprite] && (sourceRect[currentActiveSprite].Y + sourceRect[currentActiveSprite].Height) <= sheetHeights[currentActiveSprite] && animationCounter[currentActiveSprite] <= frameAmounts[currentActiveSprite] )
                {
                    sourceRect[currentActiveSprite].X += sourceRect[currentActiveSprite].Width;
                    animationCounter[currentActiveSprite]++;
                }
                else
                {
                    Hero.setBodyNull();
                    Hero.setLegNull();
                    resetAnimationValues();
                }
                 * */
            }
        }


    }
}
