using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace discord_bot.Configuration
{
    internal class ConfigReader
    {
        public string Token { get; set; }
        public string Prefix { get; set; }

        public async Task ReadJSON()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JsonStructure data = JsonConvert.DeserializeObject<JsonStructure>(json);

                this.Token = data.token;
                this.Prefix = data.prefix;
            }
        }
    }

    internal sealed class JsonStructure
    {
        public string token { get; set; }
        public string prefix { get; set; }
    }

}
