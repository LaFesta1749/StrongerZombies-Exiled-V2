using System;

using Exiled.API.Features;

using StrongerZombies.Handlers;

namespace StrongerZombies
{
    public class StrongerZombies : Plugin<BalanceSettings>
    {
        public override string Name { get; } = "StrongerZombiesv2";
        public override string Author { get; } = "LaFesta1749";
        public override string Prefix { get; } = "strongerzombiesv2";
        public override Version Version { get; } = new Version(1, 0, 1);
        public override Version RequiredExiledVersion => new Version(9, 6, 0);

        private ZombieHandler? _handlers;

        public override void OnEnabled()
        {
            Log.Debug("Initializing any event handlers...");
            _handlers = new ZombieHandler(this);

            _handlers.Subscribe();
        }

        public override void OnDisabled()
        {
            _handlers?.Unsubscribe();

            _handlers = null;
        }
    }
}