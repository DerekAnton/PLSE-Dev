using System;
using System.IO;
using System.Xml;

namespace PLSE_Project
{
    class XMLSaver
    {
        public static String GetMyDocumentsDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

        //tests to see if My Games exists in My Documents
        public static void checkDirectory(string[] args)
        {
            string myDocs = GetMyDocumentsDir();
            Random random = new Random();

            DirectoryInfo dirInfo = new DirectoryInfo(@myDocs + "\\My Games");

            //if My Games doesnt exist create the folder
            if (!dirInfo.Exists)
            {
                // Specify a "currently active folder"
                string activeDir = @myDocs;

                //Creates a string for a new subfolder My Games in My Documents
                string newPath = System.IO.Path.Combine(activeDir, "My Games");

                // Create the My Games Folder
                System.IO.Directory.CreateDirectory(newPath);
            }

            myDocs += "\\My Games";
            dirInfo = new DirectoryInfo(@myDocs + "\\PSLE Games");
            if (!dirInfo.Exists)
            {
                // updates the path afer My Games was added
                string activeDir = @myDocs;

                //Create a path for the new subfolder PSLE Games under the My Games folder
                string newPath = System.IO.Path.Combine(activeDir, "PSLE Games");

                // Create the PSLE Games folder
                System.IO.Directory.CreateDirectory(newPath);

            }
            myDocs += "\\PSLE Games";


            // Create a new file in My Documents\My Games\PSLE Games
            XmlTextWriter textWriter = new XmlTextWriter(myDocs + "\\myXmFile.xml", null);
            XmlTextReader textReader = new XmlTextReader(myDocs + "\\myXmFile.xml");


            // Opens the document
            textWriter.WriteStartDocument();

            // Write comments
            textWriter.WriteComment("First Comment XmlTextWriter Sample Example");
            textWriter.WriteComment("myXmlFile.xml in " + myDocs);

            // Write first element
            textWriter.WriteStartElement("Level");
            textWriter.WriteComment("Name Of Level");

            textWriter.WriteStartElement("Name", "");
            textWriter.WriteString("Level N");
            textWriter.WriteEndElement();

            // Ends the document.
            textWriter.WriteEndDocument();

            // close writer
            textWriter.Close();
        }
    }
}
