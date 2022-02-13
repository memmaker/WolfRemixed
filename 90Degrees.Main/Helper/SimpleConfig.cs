using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace XNAHelper
{
    public class SimpleConfig
    {
        private readonly string mFilename;
        private Dictionary<string, object> mConfig;
        public SimpleConfig(string filename)
        {
            mFilename = filename;
            mConfig = new Dictionary<string, object>();
        }

        public void WriteToConfigFile()
        {
            using StreamWriter sw = new StreamWriter(mFilename);
            foreach (KeyValuePair<string, object> keyValuePair in mConfig)
            {
                sw.WriteLine(keyValuePair.Key + " = " + keyValuePair.Value);
            }
        }

        public void ReadFromConfigFile()
        {
            using StreamReader sr = new StreamReader(mFilename);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Trim().StartsWith("#")) continue;

                string[] split = line.Split('=');
                string key = split[0].Trim();
                string value = split[1].Trim();

                if (int.TryParse(value, out int intValue))
                {
                    mConfig[key] = intValue;
                }
                else if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
                {
                    mConfig[key] = floatValue;
                }
                else  // string
                {
                    mConfig[key] = value;
                }
            }

        }

        public void Set(string key, object value)
        {
            mConfig[key] = value;
        }

        public string GetString(string key)
        {
            return (string)mConfig[key];
        }

        public int GetInt(string key)
        {
            return (int)mConfig[key];
        }

        public float GetFloat(string key)
        {
            return (float)mConfig[key];
        }
    }
}
