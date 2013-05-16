using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PLSE_Project
{
    class Animation
    {
        private Texture2D spriteSheet;
        private Rectangle sourceRect;
        private int width, height, maxFrames, frame;
        
        private bool loop;

        private double timer, timePerFrame;

        private int framesWide, framesHigh;

        private bool paused = false;


        public Animation(int frameWidth, int frameHeight, ContentManager content, string spriteSheetPath, int sheetTotalFrames, bool loopAnimation, double frameAnimationTime)
        {
            spriteSheet = content.Load<Texture2D>(spriteSheetPath);
            sourceRect = new Rectangle(0, 0, frameWidth, frameHeight);
            frame = 0;
            maxFrames = sheetTotalFrames;

            width = frameWidth;
            height = frameHeight;

            loop = loopAnimation;

            timePerFrame = frameAnimationTime;

            framesWide = spriteSheet.Width / width;
            framesHigh = spriteSheet.Height / height;
        }

        public void update(double elapsedGameTime)
        {
            if(!paused)
                timer += elapsedGameTime;

            if (timer >= timePerFrame)
            {
                timer -= timePerFrame;

                if (frame < maxFrames)
                    frame++;
                else if (loop)
                {
                    frame = 0;
                    sourceRect.X = 0;
                    sourceRect.Y = 0;
                }

                if (sourceRect.Right < spriteSheet.Width)
                    sourceRect.X += width;
                else
                {
                    sourceRect.X = 0;
                    sourceRect.Y += height;
                }

                if (frame < maxFrames)
                    frame++;
            }
        }

        public void reset()
        {
            sourceRect.X = 0;
            sourceRect.Y = 0;
            frame = 0;
        }

        public void draw(SpriteBatch spritebatch, int x, int y)
        {
            spritebatch.Draw(spriteSheet, new Vector2 (x,y), sourceRect, Color.White);
        }

        public void draw(SpriteBatch spritebatch, int x, int y, float rotation, Vector2 origin, float scale, float layerDepth)
        {
            spritebatch.Draw(spriteSheet, new Vector2(x,y), sourceRect, Color.White, rotation, origin, scale, SpriteEffects.None, layerDepth);
        }

        public void pause()
        {
            paused = true;
        }

        public void unpause()
        {
            paused = false;
        }

        public bool finishedAnimation()
        {
            return frame >= maxFrames;
        }

    }
}
