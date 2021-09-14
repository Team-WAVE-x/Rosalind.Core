using Newtonsoft.Json;
using System.IO;

namespace Rosalind.Core.Models
{
    public class Setting
    {
        [JsonProperty("config")]
        public Config Config { get; set; }

        [JsonProperty("commandGroup")]
        public CommandGroup[] CommandGroup { get; set; }

        public void GetConfig(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            var json = JsonConvert.DeserializeObject<Setting>(jsonString);

            this.Config = json.Config;
            this.CommandGroup = json.CommandGroup;
        }
    }

    public partial class Config
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("trnToken")]
        public string TrnToken { get; set; }

        [JsonProperty("koreanbotsToken")]
        public string KoreanbotsToken { get; set; }

        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }

        [JsonProperty("developerId")]
        public ulong DeveloperId { get; set; }

        [JsonProperty("koreanbotsId")]
        public string KoreanbotsId { get; set; }

        [JsonProperty("errorLogChannelId")]
        public ulong ErrorLogChannelId { get; set; }

        [JsonProperty("prefix")]
        public string Prefix { get; set; }
    }

    public class CommandGroup
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("commands")]
        public Command[] Commands { get; set; }
    }

    public partial class Command
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("args")]
        public string[] Args { get; set; }
    }
}
