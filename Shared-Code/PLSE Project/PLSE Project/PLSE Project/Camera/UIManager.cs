using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PLSE_Project
{
    class UIManager
    {
        private static Texture2D healthBarFront; //health the player has
        private static Texture2D healthBarBack; //lost health

        private static Texture2D energyBarFront;
        private static Texture2D energyBarBack;

        private const int HEALTH_BUFFER_TOP = 40;
        private const int HEALTH_BUFFER_LEFT = 50;

        private const int ENERGY_BUFFER_TOP = 140;
        private const int ENERGY_BUFFER_LEFT = 50;
        private const int ENERGY_REGEN_RATE = 2;

        private static Rectangle healthBarRect;
        private static Rectangle healthBarDrawRect;

        private static Rectangle energyBarRect;
        private static Rectangle energyBarDrawRect;

        public static void load(ContentManager content)
        {
            healthBarFront = content.Load<Texture2D>("Sprites//UI//HealthBarFront");
            healthBarBack = content.Load<Texture2D>("Sprites//UI//HealthBarBack");

            energyBarFront = content.Load<Texture2D>("Sprites//UI//EnergyBarFront");
            energyBarBack = content.Load<Texture2D>("Sprites//UI//EnergyBarBack");

            healthBarRect = new Rectangle(HEALTH_BUFFER_LEFT, HEALTH_BUFFER_TOP, healthBarBack.Width, healthBarBack.Height);
            healthBarDrawRect = new Rectangle(HEALTH_BUFFER_LEFT, HEALTH_BUFFER_TOP, healthBarBack.Width, healthBarBack.Height);

            energyBarRect = new Rectangle(ENERGY_BUFFER_LEFT, ENERGY_BUFFER_TOP, healthBarBack.Width, healthBarBack.Height);
            energyBarDrawRect = new Rectangle(ENERGY_BUFFER_LEFT, ENERGY_BUFFER_TOP, energyBarBack.Width, energyBarBack.Height);
        }

        public static void update(bool usingMagic)
        {
            healthBarDrawRect.Width = (int)(Hero.getHealth() / Hero.getMaxHealth() * healthBarRect.Width);
            energyBarDrawRect.Width = (int)(Hero.getEnergy() / Hero.getMaxEnergy() * energyBarRect.Width);
        }
        public static void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(healthBarBack, healthBarRect, Color.White);
            spriteBatch.Draw(healthBarFront, healthBarDrawRect, Color.White);

            spriteBatch.Draw(energyBarBack, energyBarRect, Color.White);
            spriteBatch.Draw(energyBarFront, energyBarDrawRect, Color.White);
        }
    }
}
