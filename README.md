# StrongerZombies

[![Downloads](https://img.shields.io/github/downloads/LaFesta1749/StrongerZombies-Exiled-V2/total?label=Downloads&color=333333&style=for-the-badge)](https://github.com/LaFesta1749/StrongerZombies-Exiled-V2/releases/latest) 
[![Discord](https://img.shields.io/badge/Discord-Join-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/PTmUuxuDXQ)

A plugin for [SCP: Secret Laboratory](https://scpslgame.com/) using the [Exiled](https://github.com/ExMod-Team/EXILED) framework.

## 📌 Description
StrongerZombies empowers SCP-049-2 (zombies) with a unique ability: when grouped together, they can break or open doors, depending on configurable rules. This plugin introduces teamwork mechanics for SCP-049-2 and adds more tension for human players.

---

## ⚙ Features
- Zombies can open or break doors when enough of them are nearby.
- Configurable door interaction behavior (Open, Break, OpenThenLock, Nothing).
- Separate settings for gates and doors.
- Cooldowns and proximity checks to prevent spam.
- Broadcast feedback when the ability is on cooldown or zombie count is insufficient.
- Full customization in `config.yml`.
- Debug mode for detailed logging.

---

## 🔧 Configuration (example)
```yaml
is_enabled: true
debug: false
breakable_door_modifier: OpenThenLock
pryable_gate_modifier: Pry
zombies_needed: 5
ability_cooldown: 24.0
max_distance: 18.9225
unlock_after_seconds: 3.0
rate_limit: 2.5
display_duration: 5
not_enough_zombies_text: "<color=red>There is not enough zombies for this ability! You need {zombiecount} zombies to open this door</color>"
on_cooldown_text: "<color=red>This ability is currently on cooldown!</color>"
```

---

## 📁 Installation
1. Download the latest release from the [Releases](https://github.com/LaFesta1749/StrongerZombies-Exiled-V2/releases) tab.
2. Place the `.dll` file into your `Exiled/Plugins` folder.
3. Start your server once to generate the config.
4. Tweak the settings in `Exiled/Configs/port-config.yml`.

---

## 👤 Author
**LaFesta1749**  
Original plugin idea based on teamwork-focused gameplay for zombies.

---

## 🧠 Credits
- Built using Exiled 9.6.0-beta7
- Special thanks to the Exiled Team and SCP:SL Modding Community

---

> "Zombies have feelings too... and now, teamwork."
> — Unknown SCP Researcher

