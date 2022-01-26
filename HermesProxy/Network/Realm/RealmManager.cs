using System.Collections.Generic;

using HermesProxy.Framework.Logging;

namespace HermesProxy.Network.Realm
{
    public static class RealmManager
    {
        public static List<RealmInfo> Realms { get; set; } = new();

        /// <summary>
        /// Adds a <see cref="RealmInfo"/> instance to the <see cref="Realms"/> instance.
        /// </summary>
        public static void AddRealm(RealmInfo realmInfo) 
            => Realms.Add(realmInfo);

        /// <summary>
        /// Prints the <see cref="RealmInfo"/> from the <see cref="Realms"/> instance.
        /// </summary>
        public static void PrintRealmList()
        {
            if (Realms.Count == 0)
                return;

            Log.Print(LogType.Debug, "");
            Log.Print(LogType.Debug, $"{"Type",-5} {"Type",-5} {"Locked",-8} {"Flags",-10} {"Name",-15} {"Address",-15} {"Port",-10} {"Build",-10}");

            foreach (var realm in Realms)
                Log.Print(LogType.Debug, realm.ToString());

            Log.Print(LogType.Debug,"");
        }
    }
}
