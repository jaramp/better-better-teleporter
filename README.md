# BetterBetterTeleporter

BetterBetterTeleporter is a mod for Lethal Company that adds configurable features for both the regular Teleporter as well as the Inverse Teleporter. The default configuration matches the unmodded version of the game, so this mod will not change any behavior unless you configure it.

## Features

- **Configurable Cooldowns:** Adjust the cooldown times for each teleporter.
- **Item Teleport Behavior:** Choose whether items are kept or dropped when using the teleporters.
- **Item Whitelists/Blacklists:** Specify comma-separated lists of items that are always kept or dropped, overriding the general item teleport behavior.
- **Reset Cooldown on Orbit:** Automatically reset teleporter cooldowns when returning the ship to orbit.
- **Inverse Teleporter Battery Drain:** An optional penalty to decrease battery charge for held items when using the Inverse Teleporter.
- **General improvements for Teleporters**:
  - Fixes a visual issue where teleporting a player that's already standing on the teleporter makes them disappear

## Configuration

The mod is configured using BepInEx's configuration system. You can modify the settings in the `BepInEx/config/BetterBetterTeleporter.cfg` file.

If [LethalConfig](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/) is installed, you can also configure the mod in-game through the LethalConfig menu.

### Configuration Options

| Option                      | Value         | Default | Description                                                                             |
| --------------------------- | ------------- | ------- | --------------------------------------------------------------------------------------- |
| ResetCooldownOnOrbit        | `Boolean`     | `false` | Resets the cooldown time on teleporters between days.                                   |
|                             |               |         | &nbsp;                                                                                  |
| TeleporterCooldown          | `Integer`     | `10`    | Cooldown time (in seconds) for using the Teleporter.                                    |
| TeleporterBehavior          | `Drop`/`Keep` | `Drop`  | Sets whether items are kept or dropped when using the Teleporter.                       |
| TeleporterAlwaysKeep        | `ItemList`    |         | Treat these items as `Keep` regardless of Teleporter behavior.                          |
| TeleporterAlwaysDrop        | `ItemList`    |         | Treant these items as `Drop` regardless of Teleporter behavior.                         |
|                             |               |         | &nbsp;                                                                                  |
| InverseTeleporterCooldown   | `Integer`     | `210`   | Cooldown time (in seconds) for using the Inverse Teleporter.                            |
| InverseTeleporterBehavior   | `Drop`/`Keep` | `Drop`  | Sets whether items are kept or dropped when using the Inverse Teleporter.               |
| InverseTeleporterAlwaysKeep | `ItemList`    |         | Treat these items as `Keep` regardless of Inverse Teleporter behavior.                  |
| InverseTeleporterAlwaysDrop | `ItemList`    |         | Treat these items as `Drop` regardless of Inverse Teleporter behavior.                  |
| BatteryDrainPercent         | `Integer`     | `0`     | Percent drain on held battery items when using the Inverse Teleporter (`0` to disable). |

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

###

|                    |     |                   |                     |                   |                    |
| ------------------ | --- | ----------------- | ------------------- | ----------------- | ------------------ |
| `BeltBag`          | ┊   | `7Ball`           | `Dentures`          | `MagnifyingGlass` | `StickyNote`       |
| `Boombox`          | ┊   | `Airhorn`         | `DiyFlashbang`      | `MetalSheet`      | `StopSign`         |
| `ExtensionLadder`  | ┊   | `BabyKiwiEgg`     | `DustPan`           | `MoldPan`         | `StunGrenade`      |
| `Flashlight`       | ┊   | `Bell`            | `EasterEgg`         | `Mug`             | `TeaKettle`        |
| `GunAmmo`          | ┊   | `BigBolt`         | `EggBeater`         | `PerfumeBottle`   | `ToiletPaperRolls` |
| `Jetpack`          | ┊   | `Binoculars`      | `EnginePart1`       | `Phone`           | `Toothpaste`       |
| `Key`              | ┊   | `BottleBin`       | `FancyCup`          | `PickleJar`       | `ToyCube`          |
| `LockPicker`       | ┊   | `Brush`           | `FancyLamp`         | `PillBottle`      | `ToyTrain`         |
| `ProFlashlight`    | ┊   | `Candy`           | `FancyPainting`     | `PlasticCup`      | `TragedyMask`      |
| `RadarBooster`     | ┊   | `CashRegister`    | `FishTestProp`      | `Ragdoll`         | `WhoopieCushion`   |
| `Shotgun`          | ┊   | `CaveDwellerBaby` | `FlashLaserPointer` | `RedLocustHive`   | `YieldSign`        |
| `Shovel`           | ┊   | `ChemicalJug`     | `Flask`             | `Remote`          | `Zeddog`           |
| `SprayPaint`       | ┊   | `Clipboard`       | `GarbageLid`        | `Ring`            |                    |
| `StunGrenade`      | ┊   | `Clock`           | `GiftBox`           | `RobotToy`        |                    |
| `TZPInhalant`      | ┊   | `ClownHorn`       | `GoldBar`           | `RubberDuck`      |                    |
| `WalkieTalkie`     | ┊   | `Cog1`            | `Hairdryer`         | `SoccerBall`      |                    |
| `WeedKillerBottle` | ┊   | `ComedyMask`      | `Knife`             | `SodaCanRed`      |                    |
| `ZapGun`           | ┊   | `ControlPad`      | `LungApparatus`     | `SteeringWheel`   |                    |

This is not an exhaustive list: any item should work, including from other mods.

BetterBetterTeleporter will do its best to resolve items with inconsistent names
(example: "clipboard" vs "Clipboard" vs "ClipboardItem"). As long as you have a
reasonable idea of what the item is called, it should work. Using the in-game
display name will also work, but it could have issues between players that use
different language settings.

If you have LethalConfig installed, there is a button at the bottom of the
BetterBetterTeleporter config section to display the names of all items you're currently
holding in your inventory. This should help verify if you're using the correct item name.

### Specifying Items by Filter

There are predefined item filters you can use to describe items or groups of items. Here is the current list of item filters:

| Filter        | Matches                               | Alternate      | Alternate Matches                                  |
| ------------- | ------------------------------------- | -------------- | -------------------------------------------------- |
| `[all]`       | All items.                            | `[none]`       | No items.                                          |
| `[battery]`   | Items that have batteries.            | `[nonbattery]` | Items that don't have batteries.                   |
| `[charged]`   | Battery items with remaining charge.  | `[discharged]` | Battery items with no charge.                      |
| `[gordion]`   | All items while on company moon.      | `[gordioff]`   | All items while not on company moon.               |
| `[held]`      | Item that is currently being held.    | `[pocketed]`   | Items not held in hand.                            |
| `[metal]`     | Items that are conductive.            | `[nonmetal]`   | Non-conductive items.                              |
| `[scrap]`     | Items the game classifies as scrap.   | `[nonscrap]`   | Items not classified as scrap.                     |
| `[twohanded]` | Items held with two hands.            | `[onehanded]`  | Items held with one hand.                          |
| `[value]`     | Items that sell for credits.          | `[worthless]`  | Items that sell for zero credits (including keys). |
| `[weapon]`    | Items the game classifies as weapons. | `[nonweapon]`  | Items not classified as weapons.                   |
| `[weighted]`  | Items that have a weight.             | `[weightless]` | Items with zero weight.                            |

As an example, take this configuration:

```ini
InverseTeleporterBehavior = Drop
InverseTeleporterAlwaysKeep = key,clipboard,[held]
```

This setting makes the Inverse Teleporter drop all items except for the `Key` and `Clipboard`,
as well as the currently-selected inventory slot (so if the player is actively holding a `GoldBar`,
they keep it, but if the `GoldBar` is not the active inventory item, it drops).


#### Restricting item filters

To further restrict an item filter, you can specify a list of items using `:`. If you wanted only
a held `Key` or `LockPicker` to be brought into the Inverse Teleporter and to drop everything else
(including keys or lockpickers that aren't being held), you can do this:

```ini
InverseTeleporterBehavior = Drop
InverseTeleporterAlwaysKeep = [held:key,lockpicker]
```

Now the `[held]` filter will only apply to keys and lockpickers, and no longer keep the held item
if it's something else. You could also use other filters such as `[held:[weightless]]` to apply to
the held item only if it's also weightless.

#### Excluding from item filters

Alternatively for any item filter, you can specify items to exclude from that filter
using the `:not` attribute. If you wanted to disallow the `Shovel` and `StopSign` to be
brought into the Inverse Teleporter even if it's currently being held, you can do this:

```ini
InverseTeleporterBehavior = Drop
InverseTeleporterAlwaysKeep = key,clipboard,[held:not(shovel,stopsign)]
```

Now the Inverse Teleporter still keeps the `Key` and `Clipboard`, as well as the currently-held item,
UNLESS the currently-held item is a `Shovel` or `StopSign`, in which case it drops.

You can also use filters within the `:not` attribute, and they can have their own associated `:not` attribute.
Example: `[held:not([twohanded:not(ragdoll)])]` would be interpreted as "currently held item unless
it's a two-handed item that is not a player's dead body".

## Dependencies

- [BepInExPack](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/)
- (Optional) [LethalConfig](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/) for in-game configuration.

## Installation

It's recommended to install from [Thunderstore](https://thunderstore.io/c/lethal-company/p/jaramp/BetterBetterTeleporter/)
using a mod manager such as [Gale](https://github.com/Kesomannen/gale).

## Contributing

See [CONTRIBUTING.md](https://github.com/jaramp/better-better-teleporter/?tab=contributing-ov-file) for information on how to contribute to the mod.
