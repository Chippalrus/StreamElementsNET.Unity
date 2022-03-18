using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace StreamElementsNET.Unity.Parsing
{
    internal static class Redemption
    {
        public static Models.Redemption.RedemptionLatest handleRedemptionLatest(JToken json)
        {
            return new Models.Redemption.RedemptionLatest(json["name"].ToString(), json["item"].ToString(), json["itemId"].ToString(), json["type"].ToString(), json["message"].ToString());
        }
    }
}