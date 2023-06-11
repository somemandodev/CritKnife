*Crit Knife* 

The Critical Knife of Goldness gives a critical hit to scientists, killing them with one stab!

## Features

- Admins can spawn in the Crit Knife, which is a gold Combat Knife, allow one hit kills to scientists when stabbed
- Permissions can restrict who can spawn in the knife and who can use it.
- The best scenario is for admins to spawn these into custom vending machines on the map

## Permissions

Hint: To easily add all protections use the RCON command: `oxide.grant {user <username> | group <group name>} critknife.*`

- **critknife.spawn** -- Spawn a Crit Knife into your inventory. Required to be able to use the `/critknifespawn` command
- **critknife.use** -- Allows the user of the knife to get an instant kill when stabbing a scientist

## Chat Commands

- **/critknifespawn** -- Spawn in a Crit Knife to your inventory. ***(requires `critknife.spawn` permission)***

## Configuration

```json
{
  "bloodEnabled": true,
  "critChance": 1.0,
  "damageMultiplier": 10000,
  "initialCondition": 500,
  "maxCondition": 500,
  "screamingEnabled": true
}
```
