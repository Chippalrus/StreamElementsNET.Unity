using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
namespace StreamElementsNET.Unity.Parsing
{
    internal static class Raid
    {
        public static Models.Raid.Raid handleRaid(JToken json)
        {
            return new Models.Raid.Raid(json["username"].ToString(), json["providerId"].ToString(), json["displayName"].ToString(), int.Parse(json["amount"].ToString()), json["avatar"].ToString());
        }

        public static Models.Raid.RaidLatest handleRaidLatest(JToken json)
        {
            return new Models.Raid.RaidLatest(json["name"].ToString(), int.Parse(json["amount"].ToString()));
        }
    }
}
