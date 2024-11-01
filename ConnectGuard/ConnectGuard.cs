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
      	public override string Description => "Simple login protection";
      	public override string Name => "Connect Guard";
        public override Version Version => new Version(1, 0, 0, 0);
      	public ConnectGuard(Main game) : base(game)
        {
        }

        public static Config config = null!;
        private Dictionary<string, int> loginAttempts = new Dictionary<string, int>();
        private Dictionary<string, (System.Timers.Timer timer, DateTime startTime)> lockoutTimers = new Dictionary<string, (System.Timers.Timer, DateTime)>();
        private int MAX_ATTEMPTS;
        private int LOCKOUT_TIME;

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

            TShock.Log.ConsoleInfo("[Connect Guard] Configuration reloaded!");
        }

        protected override void Dispose(bool disposing)
        {
          	if (disposing)
            {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnPlayerLogin);
            }
          	base.Dispose(disposing);
        }

        private async void OnPlayerLogin(GreetPlayerEventArgs args)
        {
            var playerName = TShock.Players[args.Who].Name;
            if (args.Who < 0 || args.Who >= TShock.Players.Length || TShock.Players[args.Who] == null)
            {
                TShock.Log.ConsoleInfo("[Connect Guard] Invalid player index.");
                return;
            }

            if (lockoutTimers.ContainsKey(playerName))
            {

                TShock.Log.ConsoleInfo("[Connect Guard] {0} trying to login. Kicking...", playerName);
                var (timer, startTime) = lockoutTimers[playerName];
                var remainingTime = (int)(timer.Interval - (DateTime.Now - startTime).TotalMilliseconds) / 1000;

                await Task.Delay(1000);
                TShock.Players[args.Who].Kick($"This player is temporarily locked due to too many failed login attempts. Try again in {remainingTime} seconds.", true, true);
                return;
            }


            if (loginAttempts.ContainsKey(playerName))
                loginAttempts[playerName]++;
            else
                loginAttempts[playerName] = 1;

            if (loginAttempts[playerName] > MAX_ATTEMPTS)
            {
                TShock.Log.ConsoleInfo("[Connect Guard] {0} is temporarily locked!", playerName);
                await Task.Delay(1000);
                TShock.Players[args.Who].Kick("Too many failed login attempts. This player will be temporarily locked.", true, true);
                LockoutIP(playerName);

            }
        }


        private void LockoutIP(string playerName)
        {
            var timer = new System.Timers.Timer(LOCKOUT_TIME);
            var startTime = DateTime.Now;
            timer.Elapsed += (sender, e) => UnlockIP(playerName);
            timer.AutoReset = false;
            timer.Start();

            lockoutTimers[playerName] = (timer, startTime);
            loginAttempts.Remove(playerName);
        }


        private void UnlockIP(string playerName)
        {
            if (lockoutTimers.ContainsKey(playerName))
            {
                lockoutTimers[playerName].timer.Stop();
                lockoutTimers[playerName].timer.Dispose(); 
                lockoutTimers.Remove(playerName);
                loginAttempts.Remove(playerName);
                TShock.Log.ConsoleInfo($"[Connect Guard] {playerName} has been unlocked.");
            }
        }

    }
}