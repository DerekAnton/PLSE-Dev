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

public enum CurrentWeapon { Pistol, Rifle, Rocket};

namespace PLSE_Project
{
    class Weapon : BodyPart
    {

        public static bool pistolclipEmpty = false;
        public static bool noPistolTotalAmmo = false;

        public static bool rifleclipEmpty = false;
        public static bool noRifleTotalAmmo = false;

        public static bool rcktclipEmpty = false;
        public static bool noRcktTotalAmmo = false;


        public static readonly int PISTOL_MAX_AMMO = 500; 
        public static readonly int RIFLE_MAX_AMMO = 1000;
        public static readonly int RCKT_MAX_AMMO = 10;

        public static readonly int PISTOL_CLIP_MAX_AMMO = 6;
        public static readonly int RIFLE_CLIP_MAX_AMMO = 15;
        public static readonly int RCKT_CLIP_MAX_AMMO = 2;

        public static int ammoCount = 0;
        public static int ammoInClip = 0;

        private static int totPistolAmmo = PISTOL_MAX_AMMO;
        private static int pistolClipAmmo = PISTOL_CLIP_MAX_AMMO;


        private static int totRifleAmmo = RIFLE_MAX_AMMO;
        private static int rifleClipAmmo = RIFLE_CLIP_MAX_AMMO;


        private static int totRcktAmmo = RCKT_MAX_AMMO;
        private static int rcktClipAmmo = RCKT_CLIP_MAX_AMMO;

        public Weapon(ContentManager content, int amountOfSheets, int[] frameAmount, Rectangle[] sourceRect, string[] imgPath, Vector2 startingPos, int[] frameDelayTimes)
            : base(content, amountOfSheets, frameAmount, sourceRect, imgPath, startingPos, frameDelayTimes)
        { }



        public static int getCurrentTotalAmmo(string checkString)
        {
            if (checkString.Equals("pistol"))
                return Weapon.totPistolAmmo;
            else if (checkString.Equals("rifle"))
                return Weapon.totRifleAmmo;
            else if (checkString.Equals("rckt"))
                return Weapon.totRcktAmmo;
            else
                return 0;
        }
        public static int getCurrentClipAmmo(string checkString)
        {
            if (checkString.Equals("pistol"))
                return Weapon.pistolClipAmmo;
            else if (checkString.Equals("rifle"))
                return Weapon.rifleClipAmmo;
            else if (checkString.Equals("rckt"))
                return Weapon.rcktClipAmmo;
            else
                return 0;
        }

        public static void addTotalPistolAmmo(int newAmmo)
        {
            Weapon.totPistolAmmo += newAmmo;
            if (Weapon.totPistolAmmo > PISTOL_MAX_AMMO)
                Weapon.totPistolAmmo = PISTOL_MAX_AMMO;
            if (Weapon.totPistolAmmo > 0)
                noPistolTotalAmmo = false;
        }
        public static void addTotalRifleAmmo(int newAmmo)
        {
            Weapon.totRifleAmmo += newAmmo;
            if (Weapon.totRifleAmmo > RIFLE_MAX_AMMO)
                Weapon.totRifleAmmo = RIFLE_MAX_AMMO;
            if (Weapon.totRifleAmmo > 0)
                noRifleTotalAmmo = false;
        }
        public static void addTotalRcktAmmo(int newAmmo)
        {
            Weapon.totRcktAmmo += newAmmo;
            if (Weapon.totRcktAmmo > RCKT_MAX_AMMO)
                Weapon.totRcktAmmo = RCKT_MAX_AMMO;
            if (Weapon.totRcktAmmo > 0)
                noRcktTotalAmmo = false;
        }

        public static void consumeXPistolAmmo(int x)
        {
            Weapon.pistolClipAmmo -= x;
            if (Weapon.pistolClipAmmo <= 0)
            {
                Weapon.pistolClipAmmo = 0;
                pistolclipEmpty = true;
            }
            if (Weapon.totPistolAmmo <= 0)
            {
                Weapon.totPistolAmmo = 0;
                noPistolTotalAmmo = true;
            }
        }
        public static void consumeXRifleAmmo(int x)
        {
            Weapon.rifleClipAmmo -= x;
            if (Weapon.rifleClipAmmo <= 0)
            {
                Weapon.rifleClipAmmo = 0;
                rifleclipEmpty = true;
            }
            if (Weapon.totRifleAmmo <= 0)
            {
                Weapon.totRifleAmmo = 0;
                noRifleTotalAmmo = true;
            }
        }
        public static void consumeXRcktAmmo(int x)
        {
            Weapon.rcktClipAmmo -= x;
            if (Weapon.rcktClipAmmo <= 0)
            {
                Weapon.rcktClipAmmo = 0;
                rcktclipEmpty = true;
            }
            if (Weapon.totRcktAmmo <= 0)
            {
                Weapon.totRcktAmmo = 0;
                noRcktTotalAmmo = true;
            }
        }

        public static void fireWeapon(string checkString)
        {
            if (checkString.Equals("pistol"))
                consumeXPistolAmmo(1);
            else if (checkString.Equals("rifle"))
                consumeXRifleAmmo(1);
            else if (checkString.Equals("rckt"))
                consumeXRcktAmmo(1);
        }

        public static void reloadPistol()
        {
            totPistolAmmo -= 6 - pistolClipAmmo;
            pistolClipAmmo = PISTOL_CLIP_MAX_AMMO;

            if (totPistolAmmo < PISTOL_CLIP_MAX_AMMO)
            {
                pistolClipAmmo = totPistolAmmo;
                totPistolAmmo = 0;
                noPistolTotalAmmo = true;
            }
            if (pistolClipAmmo >= 1)
                pistolclipEmpty = false;
            else
                pistolclipEmpty = true;
        }
        public static void reloadRifle()
        {
            totRifleAmmo -= 15 - rifleClipAmmo;
            rifleClipAmmo = RIFLE_CLIP_MAX_AMMO;

            if (totRifleAmmo < RIFLE_CLIP_MAX_AMMO)
            {
                rifleClipAmmo = totRifleAmmo;
                totRifleAmmo = 0;
                noRifleTotalAmmo = true;
            }
            if (rifleClipAmmo >= 1)
                rifleclipEmpty = false;
            else
                rifleclipEmpty = true;
        }
        public static void reloadRckt()
        {
            totRcktAmmo -= 2 - rcktClipAmmo;
            rcktClipAmmo = RCKT_CLIP_MAX_AMMO;

            if (totRcktAmmo < RCKT_CLIP_MAX_AMMO)
            {
                rcktClipAmmo = totRcktAmmo;
                totRcktAmmo = 0;
                noRcktTotalAmmo = true;
            }

            if (rifleClipAmmo >= 1)
                rcktclipEmpty = false;
            else
                rcktclipEmpty = true;
        }
    }
}
