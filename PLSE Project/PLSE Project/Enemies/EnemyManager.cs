using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PLSE_Project
{
    class EnemyManager
    {
        private static LinkedList<Enemy> enemies;
        private static LinkedList<Enemy> toDelete;

        public static void addEnemy(string enemyName, int x, int y, ContentManager content)
        {
            if (enemies.Equals(null))
                enemies = new LinkedList<Enemy>();
            switch (enemyName)
            {
                case "GroundPatroller":
                    enemies.AddFirst(new GroundPatroller(x, y, content));
                    break;
                case "BloatSack":
                    enemies.AddFirst(new BloatSack(x,y,content));
                    break;
                default:
                    Console.Out.WriteLine("Enemy Type Does Not Exist!");
                    break;
            }
        }

        public static void load()
        {
            enemies = new LinkedList<Enemy>();
            toDelete = new LinkedList<Enemy>();
        }

        public static void update(double elapsedTime)
        {

            foreach(Enemy enemy in enemies)
            {
                enemy.update(elapsedTime);

                if (enemy.delete())
                    toDelete.AddFirst(enemy);

                if (!enemy.isDead())
                {
                    Projectile removeProjectile = null;

                    foreach (Projectile projectile in ProjectileManager.getActiveProjectiles())
                    {
                        //Console.Out.WriteLine(projectile.getHitbox().X + " y: " + projectile.getHitbox().Y);
                        if (enemy.intersects(projectile.getHitbox()))
                        {
                            //enemy.doDamage(1);//REMOVE THIS LINE ONCE DEREK's Code is Working
                            enemy.doDamage(ProjectileManager.getDmgCoefficient(projectile));
                            removeProjectile = projectile;
                        }
                    }

                    if (removeProjectile != null)
                        ProjectileManager.deactivateBullet(removeProjectile);
                }
            }

            if (toDelete.Count > 0)
            {
                Enemy[] deleteThese = toDelete.ToArray<Enemy>();
                for (int i = 0; i < deleteThese.Length; i++)
                {
                    Console.Out.WriteLine("DELETED IT");
                    enemies.Remove(deleteThese[i]);
                }

                deleteThese = null;
                toDelete = new LinkedList<Enemy>();
            }
        }

        public static void draw(SpriteBatch spriteBatch)
        {
            foreach(Enemy enemy in enemies)
            {
                enemy.draw(spriteBatch);
            }
        }

        public static LinkedList<Enemy> getEnemies()
        {
            return enemies;
        }
    }
}
