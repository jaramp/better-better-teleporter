## Version 1.2.1

- Delay visual fixes to compensate for possible lag

## Version 1.2.0

- Added `[filter:item]` syntax
- Added patch to fix a visual bug on the teleporter

## Version 1.1.5

- Added `[all]` item filter
- Added `[none]` item filter
- Added `[gordion]` item filter
- Added `[gordioff]` item filter

## Version 1.1.4
- Added `[battery]` item filter
- Added `[charged]` item filter
- Added `[discharged]` item filter
- Added `[metal]` item filter
- Added `[nonbattery]` item filter
- Added `[nonmetal]` item filter
- Added `[nonscrap]` item filter
- Added `[nonweapon]` item filter
- Added `[onehanded]` item filter
- Added `[pocketed]` item filter
- Added `[scrap]` item filter
- Added `[twohanded]` item filter
- Added `[value]` item filter
- Added `[weapon]` item filter
- Added `[weighted]` item filter
- Added `[weightless]` item filter
- Added `[worthless]` item filter

## Version 1.1.3

- Added experimental item filter engine
- Added `[held]` item filter

## Version 1.1.2

- Rewrote netcode from scratch

## Version 1.1.1

- Fix issue where dead players wouldn't drop some items
- Fix issue where reconnecting players would always drop all items when teleporting

## Version 1.1.0

- Using CSync for better network communication
- Changed BatteryDrainPercent to an integer range from 0 to 100
- Improving init and update performance
- Fix issue where disconnecting players wouldn't drop some items
- Fix incorrect default value on ResetCooldownOnOrbit
- Fix issue where changing server host synced incorrect settings

## Version 1.0.2

- Setting default config to match unmodded game behavior
- Fix issue with incorrect carry weight after teleporting

## Version 1.0.1

- Hotfix to fix packaged file structure

## Version 1.0.0

Initial Release

- Reset cooldowns on orbit
- Customize Teleporter cooldown
- Customizable list of items to keep on Teleport
- Customize Inverse Teleporter cooldown
- Customizable list of items to keep on Inverse Teleport
- Customizable battery drain on Inverse Teleport
