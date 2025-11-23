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

| Option                      | Value         | Default | Description                                                                             |
| --------------------------- | ------------- | ------- | --------------------------------------------------------------------------------------- |
| ResetCooldownOnOrbit        | `Boolean`     | `false` | Resets the cooldown time on teleporters between days.                                   |
| -                           | -             | -       | -                                                                                       |
| TeleporterCooldown          | `Integer`     | `10`    | Cooldown time (in seconds) for using the Teleporter.                                    |
| TeleporterBehavior          | `Drop`/`Keep` | `Drop`  | Sets whether items are kept or dropped when using the Teleporter.                       |
| TeleporterAlwaysKeep        | `ItemList`    |         | Treat these items as `Keep` regardless of Teleporter behavior.                          |
| TeleporterAlwaysDrop        | `ItemList`    |         | Treant these items as `Drop` regardless of Teleporter behavior.                         |
| -                           | -             | -       | -                                                                                       |
| InverseTeleporterCooldown   | `Integer`     | `210`   | Cooldown time (in seconds) for using the Inverse Teleporter.                            |
| InverseTeleporterBehavior   | `Drop`/`Keep` | `Drop`  | Sets whether items are kept or dropped when using the Inverse Teleporter.               |
| InverseTeleporterAlwaysKeep | `ItemList`    |         | Treat these items as `Keep` regardless of Inverse Teleporter behavior.                  |
| InverseTeleporterAlwaysDrop | `ItemList`    |         | Treat these items as `Drop` regardless of Inverse Teleporter behavior.                  |
| BatteryDrainPercent         | `Integer`     | `0`     | Percent drain on held battery items when using the Inverse Teleporter (`0` to disable). |
|                             |               |         |                                                                                         |

As an example, if you wanted an Inverse Teleporter that drops all items except for keys and walkie-talkies, and drains batteries by 25%:

```ini
InverseTeleporterCooldown = 210
InverseTeleporterBehavior = Drop
InverseTeleporterAlwaysKeep = Key,WalkieTalkie
InverseTeleporterAlwaysDrop =
BatteryDrainPercent = 25
```

## Configuring Item Lists

There are two ways to specify items: by name or by filter. You can specify multiple items/filters by separating them by commas
(such as `shovel,key,[held],walkietalkie`). Names/filters are not case-sensitive (`Key`, `KEY` and `key` all work).

### Specifying Items by Name

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

BetterBetterTeleporter will do its best to resolve items with inconsistent names
(example: "clipboard" vs "Clipboard" vs "ClipboardItem"). As long as you have a
reasonable idea of what the item is called, it should work. Using the in-game
display name will also work, but it could have issues between players that use
different language settings.

If you have LethalConfig installed, there is a button at the bottom of the
BetterBetterTeleporter config section to display the names of all items you're currently
holding in your inventory. This should help verify if you're using the correct item name.

### (EXPERIMENTAL) Specifying Items by Filter

Disclaimer: this feature is being actively developed and may change in the future.

There are predefined item filters you can use to describe items or groups of items. Here is the current list of item filters:

| Filter Name    | Description                                                    |
| -------------- | -------------------------------------------------------------- |
| `[battery]`    | Items that have batteries. Opposite of `nonbattery`.           |
| `[charged]`    | Battery items with remaining charge. Opposite of `discharged`. |
| `[discharged]` | Battery items with no charge. Opposite of `charged`.           |
| `[held]`       | The currently-held item. Opposite of `pocketed`.               |
| `[metal]`      | Items that are conductive. Opposite of `nonmetal`.             |
| `[nonbattery]` | Items that do not use batteries. Opposite of `batteries`.      |
| `[nonmetal]`   | Items that are nonconductive. Opposite of `metal`.             |
| `[nonscrap]`   | Items not considered scrap. Opposite of `scrap`.               |
| `[nonweapon]`  | Nondamaging items. Opposite of `weapon`.                       |
| `[onehanded]`  | Items held with one hand. Opposite of `twohanded`.             |
| `[pocketed]`   | Items not held in hand. Opposite of `held`.                    |
| `[scrap]`      | Items the game considers scrap. Opposite of `nonscrap`.        |
| `[twohanded]`  | Items held with two hands. Opposite of `onehanded`.            |
| `[value]`      | Items worth credits. Opposite of `worthless`.                  |
| `[weapon]`     | Combat items.  Opposite of `nonweapon`.                        |
| `[weighted]`   | Items that have a weight. Opposite of `weightless`.            |
| `[weightless]` | Items with zero weight. Opposite of `weighted`.                |
| `[worthless]`  | Items that sell for 0 credits. Opposite of `value`.            |

As an example, take this configuration:

```ini
InverseTeleporterBehavior = Drop
InverseTeleporterAlwaysKeep = key,clipboard,[held]
```

This setting makes the Inverse Teleporter drop all items except for the `Key` and `Clipboard`,
as well as the currently-selected inventory slot (so if the player is actively holding a `GoldBar`,
they keep it, but if the `GoldBar` is not the active inventory item, it drops).

For any item filter, you can additionally specify items/filters to exclude from that filter
using the `:not` attribute. Reusing the previous example, if you wanted to disallow the `Shovel`
to be brought into the Inverse Teleporter even if it's currently being held, you can do this:

```ini
InverseTeleporterBehavior = Drop
InverseTeleporterAlwaysKeep = key,clipboard,[held:not(shovel)]
```

Now the Inverse Teleporter still keeps the `Key` and `Clipboard`, as well as the currently-held item,
UNLESS the currently-held item is a `Shovel`, in which case it drops.

## Dependencies

- [BepInExPack](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/)
- (Optional) [LethalConfig](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/) for in-game configuration.

## Installation

It's recommended to install from [Thunderstore](https://thunderstore.io/c/lethal-company/p/jaramp/BetterBetterTeleporter/)
using a mod manager such as [Gale](https://github.com/Kesomannen/gale).

## Contributing

See [CONTRIBUTING.md](https://github.com/jaramp/better-better-teleporter/?tab=contributing-ov-file) for information on how to contribute to the mod.
