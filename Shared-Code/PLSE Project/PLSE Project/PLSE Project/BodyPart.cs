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
    class BodyPart // Danton // 
    {
        public Texture2D[] spriteSheets;
        public Vector2[] originVecs;
        public Vector2[] position;
        public Vector2 direction;
        public Rectangle[] sourceRect;

        private int currentActiveSprite; // will store a passed (and casted) enum to show which sprite is active, to know what to animate.
        private int[] frameAmounts; // this is the total amount of frames per spritesheet //
        private int[] sheetWidths;
        private int[] sheetHeights;
        private int[] animationCounter;
        private int amountOfSheets; // amount of sheets in the array of texture 2Ds //
        private int[] lengthOfTimePerFrame;
        private int frameLimiter;

        

        public BodyPart(ContentManager content, int amountOfSheets, int[] frameAmount, Rectangle[] sourceRect, string[] imgPath, Vector2[] startingPos, int[] frameDelayTimes)
        {
            initalizeStagingArrays(amountOfSheets, frameAmount, sourceRect, imgPath);
            this.amountOfSheets = amountOfSheets;
            this.frameAmounts = new int[this.amountOfSheets];
            this.sourceRect = new Rectangle[this.amountOfSheets];
            this.lengthOfTimePerFrame = new int[this.amountOfSheets];
            this.animationCounter = new int[this.amountOfSheets];
            
            for (int counter = 0; counter < amountOfSheets; counter++)
            {
                spriteSheets[counter] = content.Load<Texture2D>(imgPath[counter]);
                sheetWidths[counter] = spriteSheets[counter].Width;
                sheetHeights[counter] = spriteSheets[counter].Height;
                this.sourceRect[counter] = sourceRect[counter];
                this.frameAmounts[counter] = frameAmount[counter];
                position[counter] = startingPos[counter];
                lengthOfTimePerFrame[counter] = frameDelayTimes[counter];
                originVecs[counter] = new Vector2(sourceRect[counter].Height / 2, sourceRect[counter].Width / 2);
                animationCounter[counter] = 1;
            }
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(spriteSheets[currentActiveSprite], position[currentActiveSprite], sourceRect[currentActiveSprite], Color.White, 0, originVecs[currentActiveSprite], 1.0f, SpriteEffects.None, 1.0f);
        }


        public void setPosition(int desiredSprite, int newX, int newY) // very small move, will need a massive move to move all the images in tandem //
        {
            position[desiredSprite].X = newX;
            position[desiredSprite].Y = newY;
        }
        public void moveAllLateral(int movespeed) // larger move,, used for basic debugging for now //
        {
            for (int counter = 0; counter < spriteSheets.Length; counter++)
            {
                position[counter].X += movespeed;
            }
        }
       
        public void animate(GameTime gameTime) // called in an update method  //
        {
            frameLimiter += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (frameLimiter >= lengthOfTimePerFrame[currentActiveSprite])
            {
                frameLimiter = 0;

                 // for this animation, i must subtract the the width of once bounding rect from the sheetlength, otherwise it will go one too many, and provide a "flashing" animation //
                if (sourceRect[currentActiveSprite].X < (sheetWidths[currentActiveSprite] - sourceRect[currentActiveSprite].Width) && sourceRect[currentActiveSprite].Y <= sheetHeights[currentActiveSprite] && animationCounter[currentActiveSprite] < frameAmounts[currentActiveSprite])
                {
                    sourceRect[currentActiveSprite].X += sourceRect[currentActiveSprite].Width;
                    animationCounter[currentActiveSprite]++;
                }
                else if (sourceRect[currentActiveSprite].X >= (sheetWidths[currentActiveSprite] - sourceRect[currentActiveSprite].Width) && sourceRect[currentActiveSprite].Y < sheetHeights[currentActiveSprite] && animationCounter[currentActiveSprite] < frameAmounts[currentActiveSprite]) 
                {
                    sourceRect[currentActiveSprite].X = 0;
                    sourceRect[currentActiveSprite].Y += sourceRect[currentActiveSprite].Height;
                    animationCounter[currentActiveSprite]++;
                }
                else
                {
                    resetAnimationValues();
                }
            }
        }

        public void setCurrentActiveSprite(int newVal)
        {
            currentActiveSprite = newVal;
        }

        public int getCurrentActiveSprite()
        {
            return currentActiveSprite;
        }
        public void setLengthOfTimePerFrame(int newLength, int arrayPosition)
        {

        }
        public void resetAnimationValues()
        {
            animationCounter[currentActiveSprite] = 1;
            sourceRect[currentActiveSprite].Y = 0;
            sourceRect[currentActiveSprite].X = 0;
        }
        private void initalizeStagingArrays(int amountOfSheets, int[] frameAmount, Rectangle[] sourceRect, string[] imgPath)
        {
            direction = new Vector2(0, 0);
            this.frameAmounts = new int[frameAmount.Length];
            this.sourceRect = new Rectangle[sourceRect.Length];
            this.spriteSheets = new Texture2D[imgPath.Length];

            originVecs = new Vector2[amountOfSheets];
            position = new Vector2[amountOfSheets];
            sheetWidths = new int[amountOfSheets];
            sheetHeights = new int[amountOfSheets];
        }
    }
}
