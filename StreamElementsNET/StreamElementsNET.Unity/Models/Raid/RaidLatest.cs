using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace StreamElementsNET.Unity.Models.Raid
{
    public class RaidLatest
    {
        public string Name { get; }
        public int Amount { get; }
        public RaidLatest( string name, int amount )
        {
            Name = name;
            Amount = amount;
        }
    }
}