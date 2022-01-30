using System.Collections.Generic;

using Bgs.Protocol;
using Bgs.Protocol.GameUtilities.V1;

using HermesProxy.Framework.Logging;
using HermesProxy.Framework.Util;
using HermesProxy.Network.BattleNet.REST;

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
        /// Write all Subregions to <see cref="GetAllValuesForAttributeResponse"/>
        /// </summary>
        public static void WriteSubRegions(GetAllValuesForAttributeResponse response)
        {
            foreach (var realmInfo in Realms)
            {
                response.AttributeValue.Add(new Variant
                {
                    StringValue = $"0-0-{realmInfo.ID}"                 //< Constructed from Region-Site-Realm
                });
            }
        }

        /// <summary>
        /// Compress and return the <see cref="RealmInfo"/> instance.
        /// </summary>
        public static byte[] GetRealmList()
        {
            var realmList = new RealmListUpdates();
            foreach (var realmInfo in Realms)
            {
                realmList.Updates.Add(new RealmListUpdate
                {
                    Update = new()
                    {
                        WowRealmAddress = realmInfo.ID,
                        PopulationState = 1,
                        CfgTimezonesID  = 1,
                        CfgCategoriesID = realmInfo.Timezone,
                        CfgRealmsID     = realmInfo.ID,
                        CfgConfigsID    = 1,
                        CfgLanguagesID  = 1,
                        Flags           = 32,
                        Name            = realmInfo.Name,
                        Version         = new()
                        {
                            Build       = 42069,
                            Major       = 9,
                            Minor       = 2,
                            Revision    = 0
                        }
                    },
                    Deleting = false,
                });
            }
            return JSON.Deflate("JSONRealmListUpdates", realmList);
        }

        /// <summary>
        /// Prints the <see cref="RealmInfo"/> from the <see cref="Realms"/> instance.
        /// </summary>
        public static void PrintRealmList()
        {
            if (Realms.Count == 0)
                return;

            Log.Print(LogType.Debug, "");
            Log.Print(LogType.Debug, $"{"ID",-5} {"Type",-5} {"Locked",-8} {"Flags",-10} {"Name",-15} {"Address",-15} {"Port",-10} {"Build",-10}");

            foreach (var realm in Realms)
                Log.Print(LogType.Debug, realm.ToString());

            Log.Print(LogType.Debug,"");
        }
    }
}
