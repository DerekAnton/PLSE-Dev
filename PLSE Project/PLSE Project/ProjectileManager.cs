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
    public enum CurrentProjectileType {Bullet, Splash};
    public enum BulletType { Pistol, Rifle, Rocket, Acid };

    class ProjectileManager
    {

        // STAGING ARRAYS //
        private static Vector2 startingPos;
        private static int[] spriteDelayTimes; // staging arrays that are filled with essential information for sprite loading thbat is delegated to each body part / transitional body part object //
        private static int[] frameAmounts;
        private static Rectangle[] frameRects;
        private static string[] imgPaths;
        private static int amountOfSheets = 0;
        private static readonly int MILLISECOND_DELAY = 30;

        static Random random = new Random();

        public static Projectile pistolProjectle, rifleProjectile, rcktProjectile, energyProjectile;  // these will never be added, but everytime a static add 'X' projectile to the linkedlist is called, we will get the respective data from these 'cookie cuter' objects.
        public static LinkedList<Projectile> activeProjectiles = new LinkedList<Projectile>();

        public static void loadProjectiles(ContentManager content)
        {
            initalizeStagingArrays();

            loadPistolBullet();
            pistolProjectle = new Projectile(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);

            loadRifleBullet();
            rifleProjectile = new Projectile(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);

            loadRcktBullet();
            rcktProjectile = new Projectile(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);

            loadEnergyProjectiles();
            energyProjectile = new Projectile(content, amountOfSheets, frameAmounts, frameRects, imgPaths, startingPos, spriteDelayTimes);
        }
        public static void update()
        {
            foreach (Projectile bullet in activeProjectiles)
            {
                if (!bullet.archingProjectile)
                {
                    if (!bullet.spriteFlipping && bullet.shotRight) // shot right or left
                    {
                        bullet.position.X += bullet.run;

                        bullet.shiftedPosition.X = bullet.position.X + CameraManager.getXOffset();
                        bullet.shiftedPosition.Y  = bullet.position.Y  + CameraManager.getYOffset();
                        
                        bullet.hitBox.X = (int)bullet.shiftedPosition.X;
                        bullet.hitBox.Y = (int)bullet.shiftedPosition.Y;
                        

                    }
                    else if (bullet.spriteFlipping && bullet.shotRight)
                    {
                        bullet.position.X -= bullet.run;

                        bullet.shiftedPosition.X = bullet.position.X + CameraManager.getXOffset();
                        bullet.shiftedPosition.Y = bullet.position.Y + CameraManager.getYOffset();

                        bullet.hitBox.X = (int)bullet.shiftedPosition.X;
                        bullet.hitBox.Y = (int)bullet.shiftedPosition.Y;
                    }

                    else if (bullet.shotUp) // case that a projectile was shot up
                    {
                        bullet.position.Y -= bullet.rise;

                        bullet.shiftedPosition.X = bullet.position.X + CameraManager.getXOffset();
                        bullet.shiftedPosition.Y = bullet.position.Y + CameraManager.getYOffset();

                        bullet.hitBox.X = (int)bullet.shiftedPosition.X;
                        bullet.hitBox.Y = (int)bullet.shiftedPosition.Y;
                    }
                    else if (!bullet.shotUp && !bullet.shotRight)
                    {
                        bullet.position.Y += bullet.rise;

                        bullet.shiftedPosition.X = bullet.position.X + CameraManager.getXOffset();
                        bullet.shiftedPosition.Y = bullet.position.Y + CameraManager.getYOffset();

                        bullet.hitBox.X = (int)bullet.shiftedPosition.X;
                        bullet.hitBox.Y = (int)bullet.shiftedPosition.Y;
                    }
                }
                else // for arching projectile //
                {
                    if(bullet.spriteFlipping)
                        bullet.position.X -= bullet.run;
                    else
                        bullet.position.X += bullet.run;
                    bullet.position.Y -= bullet.rise;
                    bullet.drop();

                    bullet.shiftedPosition.X = bullet.position.X + CameraManager.getXOffset();
                    bullet.shiftedPosition.Y = bullet.position.Y + CameraManager.getYOffset();

                    bullet.hitBox.X = (int)bullet.shiftedPosition.X;
                    bullet.hitBox.Y = (int)bullet.shiftedPosition.Y;
                }

            }
            if(ProjectileManager.activeProjectiles.Count != 0)
                checkProjectileCollision();
        }
        public static void draw(SpriteBatch spriteBatch)
        {
            foreach (Projectile bullet in activeProjectiles)
            {
                bullet.draw(spriteBatch, bullet.spriteFlipping, true);
            }
        }

        private static void loadPistolBullet()
        {
            amountOfSheets = 2; // total number of sheets to be loaded //

            imgPaths[(int)CurrentProjectileType.Bullet] = "Sprites//Projectiles//Guns//Pistol//pistol_bullet";
            imgPaths[(int)CurrentProjectileType.Splash] = "Sprites//Projectiles//Guns//Pistol//pistol_splash";

            frameAmounts[(int)CurrentProjectileType.Bullet] = 1;
            frameAmounts[(int)CurrentProjectileType.Splash] = 8;

            frameRects[(int)CurrentProjectileType.Bullet] = new Rectangle(0, 0, 11, 11);
            frameRects[(int)CurrentProjectileType.Splash] = new Rectangle(0, 0, 27, 43);

            for (int counter = 0; counter < amountOfSheets; counter++)
                spriteDelayTimes[counter] = MILLISECOND_DELAY;

            startingPos = new Vector2(Hero.weaponArsenal[0].position.X, Hero.weaponArsenal[0].position.Y);
        }
        private static void loadRifleBullet()
        {
            amountOfSheets = 2; // total number of sheets to be loaded //

            imgPaths[(int)CurrentProjectileType.Bullet] = "Sprites//Projectiles//Guns//Rifle//mchgun_bullet";
            imgPaths[(int)CurrentProjectileType.Splash] = "Sprites//Projectiles//Guns//Rifle//mchgun_splash";

            frameAmounts[(int)CurrentProjectileType.Bullet] = 1;
            frameAmounts[(int)CurrentProjectileType.Splash] = 5;

            frameRects[(int)CurrentProjectileType.Bullet] = new Rectangle(0, 0, 11, 11);
            frameRects[(int)CurrentProjectileType.Splash] = new Rectangle(0, 0, 19, 21);

            for (int counter = 0; counter < amountOfSheets; counter++)
                spriteDelayTimes[counter] = MILLISECOND_DELAY;

            startingPos = new Vector2(Hero.weaponArsenal[0].position.X, Hero.weaponArsenal[0].position.Y);
        }
        private static void loadRcktBullet()
        {
            amountOfSheets = 2; // total number of sheets to be loaded //

            imgPaths[(int)CurrentProjectileType.Bullet] = "Sprites//Projectiles//Guns//Rckt//rocket_bullet";
            imgPaths[(int)CurrentProjectileType.Splash] = "Sprites//Projectiles//Guns//Rckt//rocket_splash";

            frameAmounts[(int)CurrentProjectileType.Bullet] = 1;
            frameAmounts[(int)CurrentProjectileType.Splash] = 6;

            frameRects[(int)CurrentProjectileType.Bullet] = new Rectangle(0, 0, 17, 18);
            frameRects[(int)CurrentProjectileType.Splash] = new Rectangle(0, 0, 213, 217);

            for (int counter = 0; counter < amountOfSheets; counter++)
                spriteDelayTimes[counter] = MILLISECOND_DELAY;

            startingPos = new Vector2(Hero.weaponArsenal[0].position.X, Hero.weaponArsenal[0].position.Y);
        }
        private static void loadEnergyProjectiles()
        {
            amountOfSheets = 2; // total number of sheets to be loaded //

            imgPaths[(int)CurrentProjectileType.Bullet] = "Sprites//Projectiles//Guns//Energy//acid1";
            imgPaths[(int)CurrentProjectileType.Splash] = "Sprites//Projectiles//Guns//Energy//acidpop";

            frameAmounts[(int)CurrentProjectileType.Bullet] = 1;
            frameAmounts[(int)CurrentProjectileType.Splash] = 4;

            frameRects[(int)CurrentProjectileType.Bullet] = new Rectangle(0, 0, 16, 16);
            frameRects[(int)CurrentProjectileType.Splash] = new Rectangle(0, 0, 16, 15);


            for (int counter = 0; counter < amountOfSheets; counter++)
                spriteDelayTimes[counter] = MILLISECOND_DELAY;

            startingPos = new Vector2(Hero.weaponArsenal[0].position.X, Hero.weaponArsenal[0].position.Y);
        }

        private static void initalizeStagingArrays()
        {
            frameAmounts = new int[2];
            frameRects = new Rectangle[2];
            imgPaths = new string[2];
            spriteDelayTimes = new int[2];
        }

        public static void addBullet(string type, ContentManager content)
        {
            if(type.Equals("pistol") && !Weapon.noPistolTotalAmmo)
            {
                Vector2 shotPos = new Vector2(pistolProjectle.position.X - CameraManager.getXOffset(), pistolProjectle.position.Y - CameraManager.getYOffset());

                Projectile newProj = new Projectile(content, pistolProjectle.amountOfSheets, pistolProjectle.frameAmounts, pistolProjectle.sourceRect, getImgPaths(type), /*pistolProjectle.position*/shotPos, pistolProjectle.lengthOfTimePerFrame);
                newProj.spriteFlipping = Hero.spriteFlipping;

                newProj.setBulletType(BulletType.Pistol);

                setNewPistolBulletPosition(newProj);
                newProj.setRiseRun(15, 15);

                newProj.hitBox = new Rectangle((int)newProj.position.X, (int)newProj.position.Y, newProj.sourceRect[(int)CurrentProjectileType.Bullet].Width, newProj.sourceRect[(int)CurrentProjectileType.Bullet].Height);
                activeProjectiles.AddFirst(newProj);

                newProj.shiftedPosition.X = newProj.position.X;
                newProj.shiftedPosition.Y = newProj.position.Y;

                Weapon.fireWeapon(type);
            }
            else if(type.Equals("rifle") && !Weapon.noRifleTotalAmmo)
            {
                Vector2 shotPos = new Vector2(pistolProjectle.position.X - CameraManager.getXOffset(), pistolProjectle.position.Y - CameraManager.getYOffset());

                Projectile newProj = new Projectile(content, rifleProjectile.amountOfSheets, rifleProjectile.frameAmounts, rifleProjectile.sourceRect, getImgPaths(type), shotPos, rifleProjectile.lengthOfTimePerFrame);
                newProj.spriteFlipping = Hero.spriteFlipping;

                newProj.setBulletType(BulletType.Rifle);

                setNewRifleBulletPosition(newProj);

                newProj.setRiseRun(20, 20);

                newProj.hitBox = new Rectangle((int)newProj.position.X, (int)newProj.position.Y, newProj.sourceRect[(int)CurrentProjectileType.Bullet].Width, newProj.sourceRect[(int)CurrentProjectileType.Bullet].Height);
                activeProjectiles.AddFirst(newProj);

                newProj.shiftedPosition.X = newProj.position.X;
                newProj.shiftedPosition.Y = newProj.position.Y;
                
                Weapon.fireWeapon(type);
            }
                
            else if(type.Equals("rckt") && !Weapon.noRcktTotalAmmo)
            {
                Vector2 shotPos = new Vector2(pistolProjectle.position.X - CameraManager.getXOffset(), pistolProjectle.position.Y - CameraManager.getYOffset());

                Projectile newProj = new Projectile(content, rcktProjectile.amountOfSheets, rcktProjectile.frameAmounts, rcktProjectile.sourceRect, getImgPaths(type), shotPos, rcktProjectile.lengthOfTimePerFrame);
                newProj.spriteFlipping = Hero.spriteFlipping;

                newProj.setBulletType(BulletType.Rocket);

                setNewRcktBulletPosition(newProj);

                newProj.setRiseRun(10, 10);

                newProj.hitBox = new Rectangle((int)newProj.position.X, (int)newProj.position.Y, newProj.sourceRect[(int)CurrentProjectileType.Bullet].Width, newProj.sourceRect[(int)CurrentProjectileType.Bullet].Height);
                activeProjectiles.AddFirst(newProj);

                newProj.shiftedPosition.X = newProj.position.X;
                newProj.shiftedPosition.Y = newProj.position.Y;

                Weapon.fireWeapon(type);
            }
                
            else if(type.Equals("energy") && Hero.getEnergy() != 0)
            {
                Vector2 shotPos = new Vector2(pistolProjectle.position.X - CameraManager.getXOffset(), pistolProjectle.position.Y - CameraManager.getYOffset());

                Projectile newProj = new Projectile(content, energyProjectile.amountOfSheets, energyProjectile.frameAmounts, energyProjectile.sourceRect, getImgPaths(type), shotPos, energyProjectile.lengthOfTimePerFrame);
                newProj.spriteFlipping = Hero.spriteFlipping;

                newProj.setBulletType(BulletType.Acid);

                setNewEnergyBulletPosition(newProj);

                newProj.archingProjectile = true;


                if(Hero.getCurrentHeroState() == (int) BodySpriteIndex.Magic)
                    newProj.setRiseRun(10 + random.Next(2), 14 + random.Next(2)); // this is where you can tweak the arch of the acid //
                else if (Hero.getCurrentHeroState() == (int)BodySpriteIndex.MagicUp)
                    newProj.setRiseRun(15 + random.Next(2), 10 + random.Next(2)); // this is where you can tweak the arch of the acid //
                else if (Hero.getCurrentHeroState() == (int)BodySpriteIndex.CrouchMagic)
                    newProj.setRiseRun(10 + random.Next(2), 18 + random.Next(2)); // this is where you can tweak the arch of the acid //
                else if (Hero.getCurrentHeroState() == (int)BodySpriteIndex.MagicDown)
                    newProj.setRiseRun(-10 + random.Next(2), 14 + random.Next(2)); // this is where you can tweak the arch of the acid //
                else { }

                newProj.hitBox = new Rectangle((int)newProj.position.X, (int)newProj.position.Y, newProj.sourceRect[(int)CurrentProjectileType.Bullet].Width, newProj.sourceRect[(int)CurrentProjectileType.Bullet].Height);
                activeProjectiles.AddFirst(newProj);

                newProj.shiftedPosition.X = newProj.position.X;
                newProj.shiftedPosition.Y = newProj.position.Y;
            }
        }

        private static void setNewRcktBulletPosition(Projectile newProj)
        {
            if (Hero.getCurrentHeroTransitionState() == (int)BodyTransitionIndex.ShootStart)
            {
                if (Hero.spriteFlipping)
                    newProj.shotRight = true;

                newProj.position.Y -= 10;

                if (newProj.spriteFlipping)
                    newProj.position.X -= 100; // offset of the bullet to come out of the right side of the hero when he's shooting // 

            }
            else if (Hero.getCurrentHeroTransitionState() == (int)BodyTransitionIndex.ShootCrouchStart)
            {
                if (Hero.spriteFlipping)
                    newProj.shotRight = true;
                newProj.position.Y += 20;
                newProj.position.X += 25;

                if (newProj.spriteFlipping)
                    newProj.position.X -= 100; // offset of the bullet to come out of the right side of the hero when he's shooting // 

            }
            else if (Hero.getCurrentHeroTransitionState() == (int)BodyTransitionIndex.ShootUpStart)
            {
                newProj.shotRight = false;
                newProj.shotUp = true;

                newProj.position.Y -= 65;
                newProj.position.X -= 50;

                if (newProj.spriteFlipping)
                    newProj.position.X += 20; 

            }
            else if (Hero.getCurrentHeroTransitionState() == (int)BodyTransitionIndex.ShootDownStart)
            {
                newProj.shotRight = false;
                newProj.shotUp = false;

                newProj.position.Y += 65;
                newProj.position.X -= 28;
            }
        }
        private static void setNewPistolBulletPosition(Projectile newProj)
        {
            if (Hero.getCurrentHeroTransitionState() == (int)BodyTransitionIndex.ShootStart)
            {
                if(Hero.spriteFlipping)
                    newProj.shotRight = true;

                if (newProj.spriteFlipping)
                    newProj.position.X -= 100; // offset of the bullet to come out of the right side of the hero when he's shooting // 

            }
            else if (Hero.getCurrentHeroTransitionState() == (int)BodyTransitionIndex.ShootCrouchStart)
            {
                if (Hero.spriteFlipping)
                    newProj.shotRight = true;

                newProj.position.Y += 30;
                newProj.position.X += 45;

                if (newProj.spriteFlipping)
                    newProj.position.X -= 100; // offset of the bullet to come out of the right side of the hero when he's shooting // 

            }
            else if (Hero.getCurrentHeroTransitionState() == (int)BodyTransitionIndex.ShootUpStart)
            {
                newProj.shotRight = false;
                newProj.shotUp = true;

                newProj.position.Y -= 65;
                newProj.position.X -= 28;
            }
            else if (Hero.getCurrentHeroTransitionState() == (int)BodyTransitionIndex.ShootDownStart)
            {
                newProj.shotRight = false;
                newProj.shotUp = false;

                newProj.position.Y += 100;
                newProj.position.X -= 28;
            }
        }
        private static void setNewRifleBulletPosition(Projectile newProj)
        {
            if (Hero.getCurrentHeroState() == (int)BodySpriteIndex.Shoot)
            {
                newProj.position.Y += 30;
                newProj.position.X += 45;

                if (newProj.spriteFlipping)
                    newProj.position.X -= 150; // offset of the bullet to come out of the right side of the hero when he's shooting // 

            }
            else if (Hero.getCurrentHeroState() == (int)BodySpriteIndex.CrouchShoot)
            {
                newProj.position.Y += 50;
                newProj.position.X += 45;

                if (newProj.spriteFlipping)
                    newProj.position.X -= 100; // offset of the bullet to come out of the right side of the hero when he's shooting // 

            }
            else if (Hero.getCurrentHeroState() == (int)BodySpriteIndex.ShootUp)
            {
                newProj.shotRight = false;
                newProj.shotUp = true;

                newProj.position.Y -= 65;
                newProj.position.X -= 28;

                if (newProj.spriteFlipping)
                    newProj.position.X -= 20; // offset of the bullet to come out of the right side of the hero when he's shooting // 
            }
            else if (Hero.getCurrentHeroState() == (int)BodySpriteIndex.ShootDown)
            {
                newProj.shotRight = false;
                newProj.shotUp = false;

                newProj.position.Y += 100;
                newProj.position.X -= 28;
            }
        }
        private static void setNewEnergyBulletPosition(Projectile newProj)
        {
            if (Hero.getCurrentHeroState() == (int)BodySpriteIndex.Magic)
            {
                if (Hero.spriteFlipping)
                    newProj.spriteFlipping = true;

                newProj.position.Y -= 20;
                newProj.position.X += 5;

                if (newProj.spriteFlipping)
                {
                    newProj.position.X -= 100; // offset of the bullet to come out of the right side of the hero when he's shooting // 
                    newProj.run = newProj.run * -1;
                }

            }
            else if (Hero.getCurrentHeroState() == (int)BodySpriteIndex.CrouchMagic)
            {
                newProj.position.Y += 10;
                newProj.position.X += 20;

                if (newProj.spriteFlipping)
                {
                    newProj.position.X -= 100; // offset of the bullet to come out of the right side of the hero when he's shooting // 
                    newProj.run = newProj.run*-1;
                }

            }
            else if (Hero.getCurrentHeroState() == (int)BodySpriteIndex.MagicUp)
            {
                newProj.shotRight = false;
                newProj.shotUp = true;

                newProj.position.Y -= 65;
                newProj.position.X -= 28;

                if (newProj.spriteFlipping)
                {
                    newProj.position.X -= 50; // offset of the bullet to come out of the right side of the hero when he's shooting // 
                    newProj.run = newProj.run * -1;
                }
            }
            else if (Hero.getCurrentHeroState() == (int)BodySpriteIndex.MagicDown)
            {
                newProj.shotRight = false;
                newProj.shotUp = false;

                newProj.position.Y += 50;
                newProj.position.X += 5;

                if (newProj.spriteFlipping)
                {
                    newProj.position.X -= 100; // offset of the bullet to come out of the right side of the hero when he's shooting // 
                    newProj.run = newProj.run * -1;
                }
            }
        }

        public static LinkedList<Projectile> getActiveProjectiles()
        {
            return activeProjectiles;
        }

        public static int getDmgCoefficient(Projectile projectile) // returns the damage that would be dealt by the passed projectile //
        {
            if (projectile.getBulletType() == BulletType.Pistol)
                return 15; // pistol dmg //
            else if (projectile.getBulletType() == BulletType.Rifle)
                return 3;
            else if (projectile.getBulletType() == BulletType.Rocket)
                return 100;
            else if (projectile.getBulletType() == BulletType.Acid)
                return 5;
            else
                return 0;
        }

        private static string[] getImgPaths(string type)
        {
            string[] paths = new string[2];

            if(type.Equals("pistol"))
            {
                paths[(int)CurrentProjectileType.Bullet] = "Sprites//Projectiles//Guns//Pistol//pistol_bullet";
                paths[(int)CurrentProjectileType.Splash] = "Sprites//Projectiles//Guns//Pistol//pistol_splash";
            }
            else if(type.Equals("rifle"))
            {
                paths[(int)CurrentProjectileType.Bullet] = "Sprites//Projectiles//Guns//Rifle//mchgun_bullet";
                paths[(int)CurrentProjectileType.Splash] = "Sprites//Projectiles//Guns//Rifle//mchgun_splash";
            }
            else if(type.Equals("rckt"))   
            {

                paths[(int)CurrentProjectileType.Bullet] = "Sprites//Projectiles//Guns//Rckt//rocket_bullet";
                paths[(int)CurrentProjectileType.Splash] = "Sprites//Projectiles//Guns//Rckt//rocket_splash";
            }
            else if (type.Equals("energy"))
            {
                paths[(int)CurrentProjectileType.Bullet] = "Sprites//Projectiles//Guns//Energy//acid1";
                paths[(int)CurrentProjectileType.Splash] = "Sprites//Projectiles//Guns//Energy//acidpop";
            }
            return paths;
        }

        public static void checkProjectileCollision() // Main Projectile Collision Logic //
        {
            Projectile reference = null;
            Projectile distanceRef = null;

            foreach (Projectile bullet in activeProjectiles)
            {
                foreach (Rectangle obstacle in ObstacleManager.getColisionRectangles())
                {
                    if(bullet.hitBox.Intersects(obstacle))
                        reference = bullet;
                }
            }
            if(reference != null)
                deactivateBullet(reference);
        }
        public static void deactivateBullet(Projectile bullet)
        {
            ProjectileManager.activeProjectiles.Remove(bullet);
        }
    }
}
