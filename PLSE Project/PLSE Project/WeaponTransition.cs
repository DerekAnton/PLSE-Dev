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
    class WeaponTransition : TransitionBodyPart
    {
        public WeaponTransition(ContentManager content, int amountOfSheets, int[] frameAmount, Rectangle[] sourceRect, string[] imgPath, Vector2 startingPos, int[] frameDelayTimes)
            : base(content, amountOfSheets, frameAmount, sourceRect, imgPath, startingPos, frameDelayTimes)
        { }
    }
}
