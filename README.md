# BetterBetterTeleporter

BetterBetterTeleporter is a mod for Lethal Company that adds configurable features for both the regular Teleporter as well as the Inverse Teleporter. The default configuration matches the unmodded version of the game, so this mod will not do anything unless you configure it.

## Features

- **Configurable Cooldowns:** Adjust the cooldown times for each teleporter.
- **Item Teleport Behavior:** Choose whether items are kept or dropped when using the teleporters.
- **Item Whitelists/Blacklists:** Specify comma-separated lists of items that are always kept or dropped, overriding the general item teleport behavior.
- **Reset Cooldown on Orbit:** Automatically reset teleporter cooldowns when returning to orbit.
- **Inverse Teleporter Battery Drain:** An optional penalty to decrease battery charge for held items when using the Inverse Teleporter.

## Configuration

The mod is configured using BepInEx's configuration system. You can modify the settings in the `BepInEx/config/BetterBetterTeleporter.cfg` file.

If [LethalConfig](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/) is installed, you can also configure the mod in-game through the LethalConfig menu.

### Configuration Options

**General**

| Option               | Value     | Default | Description                                           |
| -------------------- | --------- | ------- | ----------------------------------------------------- |
| ResetCooldownOnOrbit | `Boolean` | `false` | Resets the cooldown time on teleporters between days. |

**Teleporter**

| Option               | Value            | Default | Description                                                       |
| -------------------- | ---------------- | ------- | ----------------------------------------------------------------- |
| TeleporterCooldown   | `Numeric`        | `10`    | Cooldown time (in seconds) for using the Teleporter.              |
| TeleporterBehavior   | `Drop` or `Keep` | `Drop`  | Sets whether items are kept or dropped when using the Teleporter. |
| TeleporterAlwaysKeep | `ItemList`       |         | Treat these items as `Keep` regardless of Teleporter behavior.    |
| TeleporterAlwaysDrop | `ItemList`       |         | Treat these items as `Drop` regardless of Teleporter behavior.    |

As an example, if you wanted a Teleporter that keeps all items except for the shovel, stop sign, and yield sign:

```ini
TeleporterCooldown = 10
TeleporterBehavior = Keep
TeleporterAlwaysKeep =
TeleporterAlwaysDrop = Shovel,StopSign,YieldSign
```

**Inverse Teleporter**

| Option                      | Value            | Default | Description                                                                             |
| --------------------------- | ---------------- | ------- | --------------------------------------------------------------------------------------- |
| InverseTeleporterCooldown   | `Numeric`        | `210`   | Cooldown time (in seconds) for using the Inverse Teleporter.                            |
| InverseTeleporterBehavior   | `Drop` or `Keep` | `Drop`  | Sets whether items are kept or dropped when using the Inverse Teleporter.               |
| InverseTeleporterAlwaysKeep | `ItemList`       |         | Treat these items as `Keep` regardless of Inverse Teleporter behavior.                  |
| InverseTeleporterAlwaysDrop | `ItemList`       |         | Treat these items as `Drop` regardless of Inverse Teleporter behavior.                  |
| BatteryDrainPercent         | `Numeric`        | `0`     | Percent drain on held battery items when using the Inverse Teleporter (`0` to disable). |

As an example, if you wanted an Inverse Teleporter that drops all items except for keys and walkie-talkies, and drains batteries by 25%:

```ini
InverseTeleporterCooldown = 210
InverseTeleporterBehavior = Drop
InverseTeleporterAlwaysKeep = Key,WalkieTalkie
InverseTeleporterAlwaysDrop =
BatteryDrainPercent = 25
```

## Configuring Item Lists

There are two ways to specify items: by category or by name.

### (EXPERIMENTAL) Specifying Items by Category

Disclaimer: this feature is being actively developed and may change in the future.

There are special keywords you can use to describe items or groups of items. Here is the current list of item categories:

| Category    | Description              |
| ----------- | ------------------------ |
| `[current]` | The currently-held item. |

As an example, take this configuration:

```ini
TeleporterBehavior = Drop
TeleporterAlwaysKeep = key,[current],clipboard
```

This setting makes the Teleporter drop all items except for the `Key` and `Clipboard`, as well as the currently-selected inventory slot
(so if the player is holding a `GoldBar`, they keep it, but if the `GoldBar` is not the active inventory item, it drops).

### Specifying Items by Name

You can specify multiple items to keep/drop by listing them separated by commas (such as `Shovel,Key,Flashlight,WalkieTalkie`).
BetterBetterTeleporter will do its best to resolve items with inconsistent names (example: "clipboard" vs "Clipboard" vs "ClipboardItem").
As long as you have a reasonable idea of what the item is called, it should work.

Here is a list of Lethal Company's internal item names that can be used for whitelisting/blacklisting items:

|                   |                     |                   |                 |                    |
| ----------------- | ------------------- | ----------------- | --------------- | ------------------ |
| `7Ball`           | `ClownHorn`         | `Flask`           | `Phone`         | `SteeringWheel`    |
| `Airhorn`         | `Cog1`              | `GarbageLid`      | `PickleJar`     | `StickyNote`       |
| `BabyKiwiEgg`     | `ComedyMask`        | `GiftBox`         | `PillBottle`    | `StopSign`         |
| `Bell`            | `ControlPad`        | `GoldBar`         | `PlasticCup`    | `StunGrenade`      |
| `BeltBag`         | `Dentures`          | `GunAmmo`         | `ProFlashlight` | `TeaKettle`        |
| `BigBolt`         | `DiyFlashbang`      | `Hairdryer`       | `RadarBooster`  | `ToiletPaperRolls` |
| `Binoculars`      | `DustPan`           | `Jetpack`         | `Ragdoll`       | `Toothpaste`       |
| `Boombox`         | `EasterEgg`         | `Key`             | `RedLocustHive` | `ToyCube`          |
| `BottleBin`       | `EggBeater`         | `Knife`           | `Remote`        | `ToyTrain`         |
| `Brush`           | `EnginePart1`       | `LockPicker`      | `Ring`          | `TragedyMask`      |
| `Candy`           | `ExtensionLadder`   | `LungApparatus`   | `RobotToy`      | `TZPInhalant`      |
| `CardboardBox`    | `FancyCup`          | `MagnifyingGlass` | `RubberDuck`    | `WalkieTalkie`     |
| `CashRegister`    | `FancyLamp`         | `MapDevice`       | `Shotgun`       | `WeedKillerBottle` |
| `CaveDwellerBaby` | `FancyPainting`     | `MetalSheet`      | `Shovel`        | `WhoopieCushion`   |
| `ChemicalJug`     | `FishTestProp`      | `MoldPan`         | `SoccerBall`    | `YieldSign`        |
| `Clipboard`       | `FlashLaserPointer` | `Mug`             | `SodaCanRed`    | `ZapGun`           |
| `Clock`           | `Flashlight`        | `PerfumeBottle`   | `SprayPaint`    | `Zeddog`           |

This is not an exhaustive list: any item should work, including from other mods.
Names are not case-sensitive. Using the in-game display name will also work,
but it could have issues between players that use different language settings.

If you have LethalConfig installed, there is a button at the bottom of the
BetterBetterTeleporter config section to display the names of all items you're currently
holding in your inventory. This should help verify if you're using the correct item name.

## Dependencies

- [BepInExPack](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/)
- (Optional) [LethalConfig](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/) for in-game configuration.

## Installation

It's recommended to install from [Thunderstore](https://thunderstore.io/c/lethal-company/p/jaramp/BetterBetterTeleporter/)
using a mod manager such as [Gale](https://github.com/Kesomannen/gale).

## Contributing

See [CONTRIBUTING.md](https://github.com/jaramp/better-better-teleporter/?tab=contributing-ov-file) for information on how to contribute to the mod.
