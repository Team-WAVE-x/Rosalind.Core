using Newtonsoft.Json;

namespace Rosalind.Core.Models
{
    public class LavalinkSetting
    {
        [JsonProperty("selfDeaf")]
        public bool SelfDeaf { get; set; }

        [JsonProperty("nodes")]
        public Node[] Nodes { get; set; }
    }

    public class Node
    {
        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("port")]
        public ushort Port { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}