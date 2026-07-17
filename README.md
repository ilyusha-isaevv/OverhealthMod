# OverhealthMod

OverhealthMod is a Terraria mod built for tModLoader that introduces an **Overhealth** mechanic—allowing a player's current health (`statLife`) to exceed their maximum health (`statLifeMax2`), displaying it as temporary, decaying overhealth (e.g., `480/400 HP`).

---

## How it works

At its core, **this mod works by removing health cap checks and health cap assignments** (such as `player.statLife < player.statLifeMax2`, `player.statLife = player.statLifeMax2`) throughout the game's code.

### Constraints

1. **Why do NOT modify `statLifeMax2` directly:**
   - **Inaccurate scaling and balance.** Many gameplay systems, items, and accessories in vanilla Terraria and other major mods scale their stats based on `statLifeMax2`. Changing this value would break these scaling mechanics.
   - **Undesired UI rendering.** If we increase `statLifeMax2`, the UI would simply show the player's base health pool as being larger rather than representing it as temporary *overhealth* above their true maximum health (e.g., no `480/400 HP` distinction, it would be `480/480 HP`).
   - **Health regeneration issues.** Natural health regeneration natively scales with and depends on max health, and it should not heal the player past their true maximum health.

2. **Why do NOT use a custom overhealth variable and intercept all damage/healing.**
   - **Crossmod Hell.** Trying to catch, intercept, and rewrite every single damage, healing, lifesteal, and projectile effect is a developer's torture and leads to bugs.

### The Solution

The best way to allow a player's health to exceed the maximum is to **remove the health caps themselves**.
- **Preserve health regeneration caps**, ensuring natural life regen still respects the standard max health cap (`statLifeMax2`) and not resets overhealth.
- For other healing methods (potions, lifesteal, special projectile/armor heals), we remove the standard `if (... statLife > statLifeMax2 ...)` and `statLife = statLifeMax2;` caps via IL editing (check out common IL edits in **`CommonIL.cs`**).
- A passive decay system gradually drains the overhealth back down to the player's max health over time.
- No netcode, health sync is handled vanilla way.
- All custom rendering is handled in **`OverhealthUI.cs`**.

## File Structure

- **`OverhealthPlayer.cs`**: Tracks individual player overhealth state, calculates and applies passive decay rates, and hooks into core player updates.
- **`OverhealthUI.cs`**: Handles drawing/rendering the overhealth indicator on the player's health bar.
- **`Utils/`**:
  - **`QuickIL.cs`**: A utility helper class that wraps method editing via `MonoModHooks.Modify` for one-line method hooks.
  - **`CommonIL.cs`**: Contains shared IL manipulation methods to find, remove, or replace vanilla health cap checks.
- **`Common/Crossmod/`**: Contains crossmod compatibility classes (e.g., `ThoriumCrossmodSystem.cs`) that apply IL edits to other mods' custom healing cap and clamping behaviors.

## How to contribute

To add support for a new mod:

1. Decompile the target mod to find references to `statLife` and `statLifeMax2` where health caps or assignments are checked (you can easily do it in [dnSpy](https://github.com/dnSpyEx/dnSpy)).
2. Create a new crossmod system class under the `Common/Crossmod/` directory, marked with the target mod's `[ExtendsFromMod("ModName")]` attribute.
3. Register IL edits using `QuickIL.EditMethod` to remove mod-specific health caps. You can use the common IL edits defined in `CommonIL.cs` for standard instructions.
4. Update project files:
   - Update `build.txt` to include the target mod in `weakReferences` if appropriate.
   - Reference the mod in `OverhealthMod.csproj` for compilation if necessary.
   - Update the description compatability table in `description_workshop.txt`.
