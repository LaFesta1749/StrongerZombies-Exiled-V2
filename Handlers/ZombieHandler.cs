using System.Collections.Generic;

using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Exiled.API.Features.Doors;
using PlayerRoles;

using Interactables.Interobjects.DoorUtils;

using MEC;

using UnityEngine;

namespace StrongerZombies.Handlers
{
    public class ZombieHandler
    {
        public ZombieHandler(StrongerZombies instance) => _core = instance;
        private string _zombiesNeededBroadcast = string.Empty;
        private string _onCooldownBroadcast = string.Empty;

        public void Subscribe()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.InteractingDoor += DoorInteract;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnd;
        }

        public void Unsubscribe()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.InteractingDoor -= DoorInteract;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnd;
        }

        private void OnWaitingForPlayers() => _roundEnded = false;

        private void OnRoundEnd(RoundEndedEventArgs ev)
        {
            _roundEnded = true;

            // We clear these on Round end as it's unpredictable whether Exiled will destroy any referenced objects before waiting for players.
            foreach (var item in _coroutines)
            {
                Timing.KillCoroutines(item);
            }

            _coroutines.Clear();
        }

        private void DoorInteract(InteractingDoorEventArgs ev)
        {
            if (_roundEnded || ev.Player.Role != RoleTypeId.Scp0492
                || ev.Door.RequiredPermissions.RequiredPermissions.HasFlagFast(KeycardPermissions.ScpOverride)
                || !ev.Door.IsKeycardDoor || ev.Door.IsLocked || ev.Door.IsOpen || _rateLimit > Time.time)
            {
                Log.Debug("Cannot Break Door: Not a Zombie, Door Is Locked, Door is a Normal Hall Door, Rate Limit, Checkpoint Door, Door is Open");
                return;
            }

            if (ev.Player.TryGetSessionVariable(CooldownTag, out float cd) && cd > Time.time)
            {
                if (!string.IsNullOrEmpty(_core.Config.OnCooldownText))
                {
                    Log.Debug("Getting ability on cooldown string");
                    _onCooldownBroadcast = _core.Config.OnCooldownText;
                    Log.Debug("Showing ability on cooldown broadcast to player(s)");
                    ev.Player.Broadcast(new Exiled.API.Features.Broadcast(_onCooldownBroadcast, _core.Config.DisplayDuration));
                }
                else
                    Log.Debug("Ability Cooldown String is empty");
                Log.Debug("Cannot Break Door: Cooldown Config");
                return;
            }

            int acceptedCount = 0;
            foreach (var player in Player.List)
            {
                if (player.Role != RoleTypeId.Scp0492)
                    continue;

                if ((player.Position - ev.Player.Position).sqrMagnitude < _core.Config.MaxDistance)
                {
                    // We store the player's ID so we can later give everyone a cooldown, not just the player who used it.
                    player.SessionVariables[OnCdTag] = ev.Player.Id;
                    acceptedCount++;
                }
            }

            if (_core.Config.ZombiesNeeded - 1 >= acceptedCount)
            {
                _rateLimit = Time.time + _core.Config.RateLimit;
                if (!string.IsNullOrEmpty(_core.Config.NotEnoughZombiesText))
                {
                    Log.Debug("Getting Not Enough Zombies String, replacing {zombiecount} if present");
                    _zombiesNeededBroadcast = _core.Config.NotEnoughZombiesText;
                    _zombiesNeededBroadcast = _zombiesNeededBroadcast.Replace("{zombiecount}", _core.Config.ZombiesNeeded.ToString());
                    Log.Debug("Showing Not Enough Zombies broadcast to player(s)");
                    ev.Player.Broadcast(new Exiled.API.Features.Broadcast(_zombiesNeededBroadcast, _core.Config.DisplayDuration));
                }
                else
                    Log.Debug("Zombie Required String is empty");
                Log.Debug("Cannot Break Door: Not Enough Zombies Config");
                Log.Debug("Zombies Required:" + _core.Config.ZombiesNeeded);
                return;
            }

            ev.IsAllowed = false;

            if (ev.Door is Gate pryableDoor)
            {
                switch (_core.Config.PryableGateModifier)
                {
                    case GateModifier.Pry:
                        pryableDoor.TryPry(ev.Player);
                        Log.Debug("Prying Gate Open");
                        break;
                    case GateModifier.OpenThenLock:
                        Open(ev.Door, true);
                        Log.Debug("Opening and Locking Gate");
                        break;
                    case GateModifier.Open:
                        Open(ev.Door);
                        Log.Debug("Opening Gate");
                        break;
                    case GateModifier.Nothing:
                        Log.Debug("Doing nothing as config is set to nothing");
                        break;
                }
            }
            else if (ev.Door is Exiled.API.Interfaces.IDamageableDoor damageableDoor)
            {
                switch (_core.Config.BreakableDoorModifier)
                {
                    case DoorModifier.Break:
                        damageableDoor.Damage(damageableDoor.Health);
                        Log.Debug("Destroying Door");
                        break;
                    case DoorModifier.OpenThenLock:
                        Open(ev.Door, true);
                        Log.Debug("Opening & Locking Door");
                        break;
                    case DoorModifier.Open:
                        Open(ev.Door);
                        Log.Debug("Opening Door");
                        break;
                    case DoorModifier.Nothing:
                        Log.Debug("Doing nothing as config is set to nothing");
                        break;
                }
            }

            foreach (var player in Player.List)
            {
                // Here we give the previously stored IDs to good use.
                if (player.TryGetSessionVariable(OnCdTag, out int id) && id == ev.Player.Id)
                {
                    player.SessionVariables[CooldownTag] = Time.time + _core.Config.AbilityCooldown;
                }
            }
        }

        private void Open(Door door, bool shouldLock = false)
        {
            door.IsOpen = true;

            if (!shouldLock)
                return;

            door.ChangeLock(Exiled.API.Enums.DoorLockType.AdminCommand);

            if (_core.Config.UnlockAfterSeconds > 0)
            {
                _coroutines.Add(Timing.CallDelayed(_core.Config.UnlockAfterSeconds, door.Unlock));
                Log.Debug("Adding coroutine for Unlocking Doors after Opening and Locking Door");
            }
        }

        private const string OnCdTag = "sz_oncd";
        private const string CooldownTag = "sz_cd";

        private readonly List<CoroutineHandle> _coroutines = new List<CoroutineHandle>();
        private readonly StrongerZombies _core;

        private float _rateLimit;
        private bool _roundEnded;

        public enum DoorModifier
        {
            OpenThenLock, Break, Open, Nothing
        }
        public enum GateModifier
        {
            Pry, OpenThenLock, Open, Nothing
        }
    }
}
