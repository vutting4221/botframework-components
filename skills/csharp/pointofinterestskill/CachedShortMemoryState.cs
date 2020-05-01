using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointOfInterestSkill
{
    public class CachedShortMemoryState
    {
        public CachedShortMemoryState(IDictionary<string, object> state = null)
        {
            State = state ?? new Dictionary<string, object>();
            Hash = ComputeHash(State);
        }

        public IDictionary<string, object> State { get; set; }

        public string Hash { get; set; }

        public bool IsChanged()
        {
            return Hash != ComputeHash(State);
        }

        internal string ComputeHash(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
