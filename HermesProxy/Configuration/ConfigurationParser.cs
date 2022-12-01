using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using Framework.Logging;

namespace HermesProxy.Configuration
{
    public class ConfigurationParser
    { 
        private readonly KeyValueConfigurationCollection _settingsCollection;

        public ConfigurationParser(KeyValueConfigurationCollection configCollection)
        {
            _settingsCollection = configCollection;
        }

        public static ConfigurationParser ParseFromFile(string configFile, Dictionary<string, string> overwrittenValues)
        {
            KeyValueConfigurationCollection settings;
            try
            {
                if (!File.Exists(configFile))
                    throw new FileNotFoundException($"File '{configFile}' was not found.");

                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile };
                var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                settings = ((AppSettingsSection)config.Sections.Get("appSettings")).Settings;
            }
            catch
            {
                Log.Print(LogType.Error, $"Fail to load config file '{configFile}'");
                throw;
            }

            // override config options with options from command line
            foreach (var pair in overwrittenValues)
            {
                settings.Remove(pair.Key);
                settings.Add(pair.Key, pair.Value);
            }

            return new ConfigurationParser(settings);
        }

        public string GetString(string key, string defValue)
        {
            KeyValueConfigurationElement s = _settingsCollection[key];
            return s?.Value ?? defValue;
        }

        public string[] GetStringList(string key, string[] defValue)
        {
            KeyValueConfigurationElement s = _settingsCollection[key];

            if (s?.Value == null)
                return defValue;

            var arr = s.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < arr.Length; i++)
                arr[i] = arr[i].Trim();

            return arr;
        }

        public bool GetBoolean(string key, bool defValue)
        {
            KeyValueConfigurationElement s = _settingsCollection[key];
            if (s?.Value == null)
                return defValue;

            bool aux;
            if (bool.TryParse(s.Value, out aux))
                return aux;

            Console.WriteLine("Warning: \"{0}\" is not a valid boolean value for key \"{1}\"", s.Value, key);
            return defValue;
        }

        public int GetInt(string key, int defValue)
        {
            KeyValueConfigurationElement s = _settingsCollection[key];
            if (string.IsNullOrEmpty(s?.Value))
                return defValue;

            int aux;
            if (int.TryParse(s.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out aux))
                return aux;

            Console.WriteLine("Warning: \"{0}\" is not a valid integer value for key \"{1}\"", s.Value, key);
            return defValue;
        }

        public TEnum GetEnum<TEnum>(string key, TEnum defValue) where TEnum : struct
        {
            KeyValueConfigurationElement s = _settingsCollection[key];
            if (string.IsNullOrEmpty(s?.Value))
                return defValue;

            int value;
            if (!int.TryParse(s.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                Console.WriteLine("Warning: \"{0}\" is not a valid integer value for key \"{1}\"", s.Value, key);
                return defValue;
            }

            if (Enum.IsDefined(typeof(TEnum), value))
                return (TEnum)(object)value;

            TEnum enumValue;
            if (Enum.TryParse(value.ToString(), out enumValue))
                return enumValue;

            Console.WriteLine("Warning: \"{0}\" is not a valid enum value for key \"{1}\", enum \"{2}\"", s.Value, key, typeof(TEnum).Name);
            return defValue;
        }

        public byte[] GetByteArray(string key, byte[] defValue)
        {
            KeyValueConfigurationElement s = _settingsCollection[key];
            if (string.IsNullOrWhiteSpace(s?.Value))
            {
                return defValue;
            }
            return s.Value.ParseAsByteArray();
        }
    }
}
