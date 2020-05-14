using Newtonsoft.Json;

namespace PointOfInterestSkill.Models
{
    public partial class Usersharedproperty
    {
        [JsonProperty("UserSharedData")]
        public UserSharedData UserSharedData { get; set; }
    }

    public partial class UserSharedData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
