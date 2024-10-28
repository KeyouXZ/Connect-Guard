using Newtonsoft.Json;
using TShockAPI;

namespace ConnectGuard {
    public class Config {
        [JsonProperty("Enable?")]
		public bool enabled { get; set; } = true;
        [JsonProperty("Max Attempt")]
        public int maxAttempts { get; set; } = 5;
        [JsonProperty("Lookout Time (Minute)")]
        public int lookoutTime { get; set; } = 1;

        public void Write() {
			string path = Path.Combine(TShock.SavePath, "ConnectGuard.json");
			File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
		}
		public static Config Read() {
			string filepath = Path.Combine(TShock.SavePath, "ConnectGuard.json");

			try {
				Config config = new Config();

				if (!File.Exists(filepath)) {
					File.WriteAllText(filepath, JsonConvert.SerializeObject(config, Formatting.Indented));
				}
				config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(filepath));


				return config;
			}
			
			catch (Exception ex) {
				TShock.Log.ConsoleError(ex.ToString());
				return new Config();
			}
		}
	}
}