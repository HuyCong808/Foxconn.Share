using Newtonsoft.Json;
using System;
using System.IO;

namespace Foxconn.AOI.Editor
{
    internal class Customization
    {
        private string _filePath = @$"{AppDomain.CurrentDomain.BaseDirectory}params\Customization.json";

        public static Customization Current => __current;
        private static Customization __current = new Customization();
        static Customization() { }
        private Customization() { }

        public static void Reload()
        {
            Customization customization = new Customization();
            Customization loaded = customization.Load();
            if (loaded != null)
            {
                __current = loaded;
            }
            else
            {
                customization.Save();
            }
        }

        public Customization Load()
        {
            Customization Customization = null;
            if (File.Exists(_filePath))
            {
                string contents = File.ReadAllText(_filePath);
                Customization = JsonConvert.DeserializeObject<Customization>(contents);
            }
            return Customization;
        }

        public void Save()
        {
            string contents = JsonConvert.SerializeObject(__current, Formatting.Indented);
            File.WriteAllText(_filePath, contents);
        }
    }
}
