using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Content;

namespace PLSE_Project
{
    class LevelReader
    {
        enum BuildType { Hero, Obstacle, Door, Boundry, Item, Enemy, GrabPoint, Platform };
        //private static bool finishedBuilding = true;
        private static string lastElementString;

        //All Build Variables
        private static int x, y, width, height, frames;
        private static string name, imgPath, imgDirectory, layer, itemType;

        public static void loadLevel(ContentManager content, int levelID)
        {
            XmlTextReader reader = new XmlTextReader("Levels/Level" + levelID + ".xml");
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        lastElementString = reader.Name;
                        break;
                    case XmlNodeType.Text:
                        addVal(reader.Name);
                        break;
                    case XmlNodeType.EndElement:
                        buildObject(reader.Name, content);
                        break;
                    default:
                        Console.WriteLine("Hit a weird XmlNodeType. It was of type: " + reader.NodeType);
                        break;
                }
            }
        }

        private static void addVal(string val)
        {
            switch (lastElementString)
            {
                case "X":
                    x = int.Parse(val);
                    break;
                case "Y":
                    y = int.Parse(val);
                    break;
                case "ImgPath":
                    imgPath = val;
                    break;
                case "ImgDirectory":
                    imgDirectory = val;
                    break;
                case "Width":
                    width = int.Parse(val);
                    break;
                case "Height":
                    height = int.Parse(val);
                    break;
                case "Frames":
                    frames = int.Parse(val);
                    break;
                case "Layer":
                    layer = val;
                    break;
                case "ItemType":
                    itemType = val;
                    break;
                case "Name":
                    name = val;
                    break;
                default:
                    Console.WriteLine("Something Went Horribly Wrong in File Loading Syntax!");
                    break;
            }
        }

        private static void buildObject(string elementName, ContentManager content)
        {
            switch (elementName)
            {
                case "Map":
                    CameraManager.addLevelRect(x, y, width, height);
                    break;
                case "Hero":
                    Hero.setX(x);
                    Hero.setY(y);
                    break;
                case "Obstacle": //terrain/platforms/background
                    ObstacleManager.addObstacle(content, imgPath, x, y, layer);
                    break;
                case "Door": //Work on door code
                    break;
                case "Enemy": //Figure out texture layout for enemies
                    break;
                case "Item": //build Item manager for addition
                    break;
                case "GrabPoint": //figure out how derek's doing ledge grabbing
                    break;
                case "CollisionRect":
                    ObstacleManager.addCollisionRectangle(x, y, width, height);
                    break;
            }
        }
    }
}
