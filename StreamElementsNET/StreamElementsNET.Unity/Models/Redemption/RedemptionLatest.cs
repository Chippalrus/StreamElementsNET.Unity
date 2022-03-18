using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamElementsNET.Unity.Models.Redemption
{
    public class RedemptionLatest
    {
        public string Name { get; }
        public string Item { get; }
        public string ItemId { get; }
        public string Type { get; }
        public string Message { get; }
        public RedemptionLatest( string name, string item, string itemId, string type, string message )
        {
            Name = name;
            Item = item;
            ItemId = itemId;
            Type = type;
            Message = message;
        }
    }
}