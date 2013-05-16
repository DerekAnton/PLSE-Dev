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
        private const int ENERGY_BUFFER_TOP = 140;
        private const int ENERGY_BUFFER_LEFT = 50;
        private const int ENERGY_REGEN_RATE = 2;

        private static Rectangle energyBarDrawRect;

        private static Texture2D pistolPipFull, pistolPipEmpty, rocketPipFull, rocketPipEmpty, riflePipFull, riflePipEmpty, acidFull, acidEmpty, healthPipFull;
        private static Texture2D acidIcon, healthIcon, pistolIcon, rocketIcon, rifleIcon;

        private const int PISTOL_PIP_SPACER = 25, HEALTHBAR_PIP_SPACER = 29, HEALTH_PIP_SPACER_LEFT = 107, HEALTH_PIP_SPACER_TOP = 30, ROCKET_PIP_SPACER = 100, RIFLE_PIP_SPACER = 20;

        private const int GUN_RIGHT_SPACER = 150, HEALTH_ICON_SPACER_LEFT = 20, HEALTH_ICON_SPACER_TOP = 20, ACID_ICON_LEFT_SPACER = 35, ACID_ICON_TOP_SPACER = 96, WEAPON_ICON_SPACER_RIGHT = 150, WEAPON_ICON_SPACER_TOP = 25;

        private const int AMMO_FONT_RIGHT_SPACER = 230, AMMO_FONT_TOP_SPACER = 20, AMMO_PIPS_RIGHT_SPACER = 80, AMMO_PIPS_TOP_SPACER = 100;

        //private static double timer;

        private static SpriteFont tempFont;
        private static Color fontColor = Color.Aquamarine;

        public static void load(ContentManager content)
        {
            importTextures(content);


            //energyBarRect = new Rectangle(ENERGY_BUFFER_LEFT, ENERGY_BUFFER_TOP, healthBarBack.Width, healthBarBack.Height);
            energyBarDrawRect = new Rectangle(110, 101, acidEmpty.Width, acidEmpty.Height);

            tempFont = content.Load<SpriteFont>("Fonts\\temp");


        }

        public static void update()
        {
            //healthBarDrawRect.Width = (int)(Hero.getHealth() / Hero.getMaxHealth() * healthBarRect.Width);
            energyBarDrawRect.Width = (int)(Hero.getEnergy() / Hero.getMaxEnergy() * acidEmpty.Width);
        }
        public static void draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(healthBarBack, healthBarRect, Color.White);
            //spriteBatch.Draw(healthBarFront, healthBarDrawRect, Color.White);

            spriteBatch.Draw(healthIcon, new Vector2(HEALTH_ICON_SPACER_LEFT, HEALTH_ICON_SPACER_TOP), Color.White);
            for (int i = 0; i < Hero.getHealth(); i++)
                spriteBatch.Draw(healthPipFull, new Vector2(HEALTH_PIP_SPACER_LEFT + HEALTHBAR_PIP_SPACER * i, HEALTH_PIP_SPACER_TOP), Color.White);

            spriteBatch.Draw(acidIcon, new Vector2(ACID_ICON_LEFT_SPACER, ACID_ICON_TOP_SPACER), Color.White);
            spriteBatch.Draw(acidEmpty, new Vector2(energyBarDrawRect.X,energyBarDrawRect.Y) , Color.White);
            spriteBatch.Draw(acidFull, energyBarDrawRect, Color.White);

            switch (Hero.getCurrentWeapon())
            {
                case CurrentWeapon.Pistol:
                    spriteBatch.Draw(pistolIcon, new Vector2(CameraManager.getViewportRect().Right - WEAPON_ICON_SPACER_RIGHT, WEAPON_ICON_SPACER_TOP), Color.White);
                    fontColor = Color.Aquamarine;
                    drawPistolPips(spriteBatch);
                    spriteBatch.DrawString(tempFont, Weapon.getCurrentTotalAmmo("pistol").ToString(), new Vector2(CameraManager.getViewportRect().Right - AMMO_FONT_RIGHT_SPACER, AMMO_FONT_TOP_SPACER), fontColor);
                    break;
                case CurrentWeapon.Rifle:
                    spriteBatch.Draw(rifleIcon, new Vector2(CameraManager.getViewportRect().Right - WEAPON_ICON_SPACER_RIGHT, WEAPON_ICON_SPACER_TOP), Color.White);
                    fontColor = Color.DarkViolet;
                    drawRiflePips(spriteBatch);
                    spriteBatch.DrawString(tempFont, Weapon.getCurrentTotalAmmo("rifle").ToString(), new Vector2(CameraManager.getViewportRect().Right - AMMO_FONT_RIGHT_SPACER, AMMO_FONT_TOP_SPACER), fontColor);
                    break;
                case CurrentWeapon.Rocket:
                    spriteBatch.Draw(rocketIcon, new Vector2(CameraManager.getViewportRect().Right - WEAPON_ICON_SPACER_RIGHT, WEAPON_ICON_SPACER_TOP), Color.White);
                    fontColor = Color.Orange;
                    drawRocketPips(spriteBatch);
                    spriteBatch.DrawString(tempFont, Weapon.getCurrentTotalAmmo("rckt").ToString(), new Vector2(CameraManager.getViewportRect().Right - AMMO_FONT_RIGHT_SPACER, AMMO_FONT_TOP_SPACER), fontColor);
                    break;
            }


        }

        private static void importTextures(ContentManager content)
        {
            pistolPipFull = content.Load<Texture2D>("Sprites\\UI\\pistol_bullet");
            rocketPipFull = content.Load<Texture2D>("Sprites\\UI\\rocket_bullet");
            riflePipFull = content.Load<Texture2D>("Sprites\\UI\\machine_bullet");
            healthPipFull = content.Load<Texture2D>("Sprites\\UI\\health_chunk");
            acidFull = content.Load<Texture2D>("Sprites\\UI\\acid_full");

            pistolPipEmpty = content.Load<Texture2D>("Sprites\\UI\\pistol_bullet_grey");
            rocketPipEmpty = content.Load<Texture2D>("Sprites\\UI\\rocket_bullet_grey");
            riflePipEmpty = content.Load<Texture2D>("Sprites\\UI\\machine_bullet_grey");
            acidEmpty = content.Load<Texture2D>("Sprites\\UI\\acid_empty");

            pistolIcon = content.Load<Texture2D>("Sprites\\UI\\pistol_icon");
            rocketIcon = content.Load<Texture2D>("Sprites\\UI\\rocket_icon");
            rifleIcon = content.Load<Texture2D>("Sprites\\UI\\machine_icon");
            healthIcon = content.Load<Texture2D>("Sprites\\UI\\health_icon");
            acidIcon = content.Load<Texture2D>("Sprites\\UI\\acid_icon");

        }

        public static void drawPistolPips(SpriteBatch spriteBatch)
        {
            int i;
            for (i = 0; i < Weapon.getCurrentClipAmmo("pistol"); i++)
            {
                spriteBatch.Draw(pistolPipFull, new Vector2(CameraManager.getViewportRect().Right - AMMO_PIPS_RIGHT_SPACER - i * PISTOL_PIP_SPACER, AMMO_PIPS_TOP_SPACER), Color.White);
            }
            for (; i < Weapon.PISTOL_CLIP_MAX_AMMO; i++)
            {
                spriteBatch.Draw(pistolPipEmpty, new Vector2(CameraManager.getViewportRect().Right - AMMO_PIPS_RIGHT_SPACER - i * PISTOL_PIP_SPACER, AMMO_PIPS_TOP_SPACER), Color.White);
            }
        }

        public static void drawRiflePips(SpriteBatch spriteBatch)
        {
            int i;
            for (i = 0; i < Weapon.getCurrentClipAmmo("rifle"); i++)
            {
                spriteBatch.Draw(riflePipFull, new Vector2(CameraManager.getViewportRect().Right - AMMO_PIPS_RIGHT_SPACER - i * RIFLE_PIP_SPACER, AMMO_PIPS_TOP_SPACER), Color.White);
            }
            for (; i < Weapon.RIFLE_CLIP_MAX_AMMO; i++)
            {
                spriteBatch.Draw(riflePipEmpty, new Vector2(CameraManager.getViewportRect().Right - AMMO_PIPS_RIGHT_SPACER - i * RIFLE_PIP_SPACER, AMMO_PIPS_TOP_SPACER), Color.White);
            }
        }

        public static void drawRocketPips(SpriteBatch spriteBatch)
        {
            int i;
            for (i = 0; i < Weapon.getCurrentClipAmmo("rckt"); i++)
            {
                spriteBatch.Draw(rocketPipFull, new Vector2(CameraManager.getViewportRect().Right - AMMO_PIPS_RIGHT_SPACER - i * ROCKET_PIP_SPACER, AMMO_PIPS_TOP_SPACER), Color.White);
            }
            for (; i < Weapon.RCKT_CLIP_MAX_AMMO; i++)
            {
                spriteBatch.Draw(rocketPipEmpty, new Vector2(CameraManager.getViewportRect().Right - AMMO_PIPS_RIGHT_SPACER - i * ROCKET_PIP_SPACER, AMMO_PIPS_TOP_SPACER), Color.White);
            }
        }

    }
}
