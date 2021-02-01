using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutoIdleMining
{
    public static class SaveState
    {
        public static string ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "\\config.xml";

        //Store values
        public static Values values = new Values();

        public class Values
        {
            //declare values to be saved
            public int idleActivate { get; set; }
            public String[] minerProcesses { get; set; }
        }

        //Saves values ready to be stored
        public static void SaveConfig(Values values)
        {
            //update values
            Values state = new Values();

            //Values
            state.idleActivate = 30000;
            state.minerProcesses = new string[] { "xmrig", "phoenixminer-eth" };

            //writes values to config
            WriteConfig(state);
        }

        //Load values from config
        public static void LoadConfig()
        {
            if (File.Exists(ConfigPath))
            {
                Values state = new Values();

                XmlSerializer serializer = new XmlSerializer(typeof(Values));
                using (FileStream fs = File.OpenRead(ConfigPath))
                {
                    state = (Values)serializer.Deserialize(fs);
                }
                //Sets application info to config values
                //TODO: change loaded values
                values.idleActivate = state.idleActivate;
                values.minerProcesses = state.minerProcesses;
            }
            else
            {
                //Create configs if they do not exist
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                File.Create(ConfigPath).Dispose();
                SaveConfig(new Values());
            }
        }

        //Store values to config
        public static void WriteConfig(Values values)
        {
            if (!File.Exists(ConfigPath))
            {
                File.Create(ConfigPath).Dispose();
            }

            //write player values to file in %appdata%
            XmlSerializer serializer = new XmlSerializer(typeof(Values));
            using (TextWriter tw = new StreamWriter(ConfigPath))
            {
                serializer.Serialize(tw, values);
            }
        }
    }
}
