using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy
{
    public static class GameData
    {
        // Storages
        public static Dictionary<uint, BroadcastText> BroadcastTextStore = new Dictionary<uint, BroadcastText>();

        // Loading code
        public static void LoadEverything(string connString)
        {
            Console.WriteLine("[GameData] Loading database...");
            LoadBroadcastTexts(connString);
            Console.WriteLine("[GameData] Finished loading database.");
        }
        public static void LoadBroadcastTexts(string connString)
        {
            BroadcastTextStore.Clear();

            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "SELECT `entry`, `male_text`, `female_text` FROM `broadcast_text` ORDER BY `entry`";
            try
            {
                conn.Open();
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    BroadcastText txt = new BroadcastText();
                    txt.Entry = reader.GetUInt32(0);
                    txt.MaleText = reader.GetString(1);
                    txt.FemaleText = reader.GetString(2);
                    BroadcastTextStore.Add(txt.Entry, txt);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GameData] An error occurred while loading broadcast texts.");
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
        }
    }

    // Data structures
    public class BroadcastText
    {
        public uint Entry;
        public string MaleText;
        public string FemaleText;
    }
}
