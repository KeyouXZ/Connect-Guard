using System.Collections.Generic;
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
        private Dictionary<string, System.Timers.Timer> lockoutTimers = new Dictionary<string, System.Timers.Timer>();
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

        private void OnPlayerLogin(GreetPlayerEventArgs args)
        {
            var playerIP = TShock.Players[args.Who].IP;
            
            if (lockoutTimers.ContainsKey(playerIP))
            {
                TShock.Players[args.Who].Disconnect("This IP is temporarily locked due to too many failed login attempts.");
                return;
            }

            if (loginAttempts.ContainsKey(playerIP))
                loginAttempts[playerIP]++;
            else
                loginAttempts[playerIP] = 1;

            if (loginAttempts[playerIP] > MAX_ATTEMPTS)
            {
                TShock.Players[args.Who].Disconnect("Too many failed login attempts. This IP will be temporarily locked.");
                LockoutIP(playerIP);
            }
        }

        private void LockoutIP(string playerIP)
        {
            var timer = new System.Timers.Timer(LOCKOUT_TIME);
            timer.Elapsed += (sender, e) => UnlockIP(playerIP);
            timer.AutoReset = false;
            timer.Start();
            
            lockoutTimers[playerIP] = timer;
            loginAttempts.Remove(playerIP);
        }

        private void UnlockIP(string playerIP)
        {
            if (lockoutTimers.ContainsKey(playerIP))
            {
                lockoutTimers[playerIP].Stop();
                lockoutTimers.Remove(playerIP);
            }
        }
    }
}