# BetterBetterTeleporter

BetterBetterTeleporter is a mod for Lethal Company that adds configurable features for both the regular Teleporter as well as the Inverse Teleporter.

## Features

- **Configurable Cooldowns:** Adjust the cooldown times for each teleporter.
- **Item Teleport Behavior:** Choose whether items are kept or dropped when using the teleporters.
- **Item Whitelists/Blacklists:** Specify lists of items that are always kept or dropped, overriding the general item teleport behavior.
- **Reset Cooldown on Orbit:** Automatically reset teleporter cooldowns when returning to orbit.
- **Inverse Teleporter Battery Drain:** An optional penalty to decrease battery charge for held items when using the Inverse Teleporter.

## Configuration

The mod is configured using BepInEx's configuration system. You can modify the settings in the `BepInEx/config/BetterBetterTeleporter.cfg` file.

If [LethalConfig](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/) is installed, you can also configure the mod in-game through the LethalConfig menu.

### Configuration Options

**General**

| Option               | Value          | Default | Description                                           |
| -------------------- | -------------- | ------- | ----------------------------------------------------- |
| ResetCooldownOnOrbit | `true`/`false` | `true`  | Resets the cooldown time on teleporters between days. |

**Teleporter**

| Option               | Value         | Default | Description                                                       |
| -------------------- | ------------- | ------- | ----------------------------------------------------------------- |
| TeleporterCooldown   | `Number`      | `10`    | Cooldown time (in seconds) for using the Teleporter.              |
| TeleporterBehavior   | `Drop`/`Keep` | `Drop`  | Sets whether items are kept or dropped when using the Teleporter. |
| TeleporterAlwaysKeep | `Text`        |         | Treat these items as `Keep` regardless of Teleporter behavior.    |
| TeleporterAlwaysDrop | `Text`        |         | Treat these items as `Drop` regardless of Teleporter behavior.    |

As an example, if you wanted a Teleporter that keeps all items except for the shovel, stop sign, and yield sign:

```ini
TeleporterCooldown = 10
TeleporterBehavior = Keep
TeleporterAlwaysKeep =
TeleporterAlwaysDrop = Shovel,StopSign,YieldSign
```

**Inverse Teleporter**

| Option                      | Value         | Default | Description                                                               |
| --------------------------- | ------------- | ------- | ------------------------------------------------------------------------- |
| InverseTeleporterCooldown   | `Number`      | `210`   | Cooldown time (in seconds) for using the Inverse Teleporter.              |
| InverseTeleporterBehavior   | `Drop`/`Keep` | `Drop`  | Sets whether items are kept or dropped when using the Inverse Teleporter. |
| InverseTeleporterAlwaysKeep | `Text`        | `Key`   | Treat these items as `Keep` regardless of Inverse Teleporter behavior.    |
| InverseTeleporterAlwaysDrop | `Text`        |         | Treat these items as `Drop` regardless of Inverse Teleporter behavior.    |
| BatteryDrainPercent         | `0.0`-`1.0`   | `0.0`   | Percent drain on held battery items when using the Inverse Teleporter.    |

As an example, if you wanted an Inverse Teleporter that drops all items except for keys and walkie-talkies, and drains batteries by 50%:

```ini
InverseTeleporterCooldown = 210
InverseTeleporterBehavior = Drop
InverseTeleporterAlwaysKeep = Key,WalkieTalkie
InverseTeleporterAlwaysDrop =
BatteryDrainPercent = 0.5
```

## Item Names

Here are some example names that can be used for whitelisting/blacklisting items:

- GunAmmo
- BeltBag
- Binoculars
- Boombox
- CardboardBox
- Clipboard
- ExtensionLadder
- Flashlight
- Jetpack
- Key
- LockPicker
- CaveDwellerBaby
- MapDevice
- ProFlashlight
- RadarBooster
- Shovel
- SprayPaint
- StickyNote
- StunGrenade
- TZPInhalant
- WalkieTalkie
- WeedKillerBottle
- ZapGun

This is not an exhaustive list. Any item should work, including from other mods.
Names are not case-sensitive. Using the in-game display name will also work,
but it may have issues when playing with players that use different language settings.
