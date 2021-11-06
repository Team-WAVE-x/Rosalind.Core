using Newtonsoft.Json;
using Rosalind.Core.Models;
using System.IO;

namespace Rosalind.Core.Modules
{
    public class SettingManager
    {
        public static Setting GetSetting(string configFilePath)
        {
            string jsonString = File.ReadAllText(Path.GetFullPath(configFilePath));
            return JsonConvert.DeserializeObject<Setting>(jsonString);
        }
    }
}
