using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using TShockAPI.Hooks;

namespace ConnectGuard
{
    [ApiVersion(2, 1)]
    public class ConnectGuard : TerrariaPlugin
    {
      	public override string Author => "Keyou";
      	public override string Description => "A sample test plugin";
      	public override string Name => "Test Plugin";
        public override Version Version => new Version(1, 0, 0, 0);
      	public ConnectGuard(Main game) : base(game)
        {
        }

        public static Config config;
        private Dictionary<string, int> loginAttempts = new Dictionary<string, int>();
        private Dictionary<string, Timer> lockoutTimers = new Dictionary<string, Timer>();
        private int MAX_ATTEMPTS;
        private int LOCKOUT_TIME = 15 * 60 * 1000;

        public override void Initialize()
        {
          	config = Config.Read();
            MAX_ATTEMPTS = config.maxAttempts;
            LOCKOUT_TIME = config.lookoutTime * 60 * 1000;
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnPlayerLogin);
        }

        public void Reload(ReloadEventArgs args) {
            config = Config.Read();
            MAX_ATTEMPTS = config.maxAttempts;
            LOCKOUT_TIME = config.lookoutTime * 60 * 1000;

            TShock.Log.ConsoleInfo("[Connect Guard] Successfully reloaded configuration!");
        }

        protected override void Dispose(bool disposing)
        {
          	if (disposing)
            {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnPlayerLogin);
            }
          	base.Dispose(disposing);
        }
    }
}