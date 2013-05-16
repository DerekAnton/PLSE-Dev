using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using System.IO;

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

        private static string[] test;

        public static void loadLevel(ContentManager content, int levelID)
        {
            string gamePath = getGameDirectory(test);

            XmlTextReader reader = new XmlTextReader(gamePath + "\\Levels\\Level" + levelID + ".xml");
            while (reader.Read())
            {
       
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        
                        //Console.Out.Write(reader.Name + ": " + reader.Value);//reader.ReadElementContentAsString());//WORKING HERE
                        lastElementString = reader.Name;
                        break;
                    case XmlNodeType.Text:
                        //Console.Out.WriteLine(reader.Value);
                        addVal(reader.ReadString());
                        break;
                    case XmlNodeType.EndElement:
                        buildObject(reader.Name, content);
                        break;
                    default:
                        if(reader.NodeType != XmlNodeType.Whitespace) {Console.WriteLine("Hit a weird XmlNodeType. It was of type: " + reader.NodeType);}
                        break;
                }
            }
        }

        private static void addVal(string val)
        {
            if(!val.Equals(""))
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
                case "Terrain": //terrain/platforms/background
                    //Console.Out.WriteLine("Building Obstacle With ImgPath: " + imgPath);
                    ObstacleManager.addObstacle(content, imgPath, x, y, layer);
                    break;
                case "Door": //Work on door code
                    break;
                case "Enemy": //Figure out texture layout for enemies
                    EnemyManager.addEnemy(name, x, y, content);
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

        private static String GetMyDocumentsDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

        //tests to see if My Games exists in My Documents
        private static string getGameDirectory(string[] args)
        {
            string directoryPath = GetMyDocumentsDir();
            DirectoryInfo dirInfo = new DirectoryInfo(@directoryPath + "\\My Games\\PLSE Games");
            
            //if PLSE Games doesnt exist create the folder
            if (!dirInfo.Exists)
            {
                XMLSaver.checkDirectory(test);
                throw new System.NullReferenceException("No Level Data Found in " + directoryPath);
            }
            else
                directoryPath += "\\My Games\\PLSE Games";

            return directoryPath;
        }
    }
}
