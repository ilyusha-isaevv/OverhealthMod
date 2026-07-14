using System;
using MonoMod.Cil;
using OverhealthMod.Common.Configs;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OverhealthMod;

public static class OverhealthExtension
{
    /// <inheritdoc cref="OverhealthPlayer.Overhealth"/>
    public static int GetOverhealth(this Player p) => Math.Max(0, p.statLife - p.statLifeMax2);
}

public class OverhealthPlayer : ModPlayer
{
    /// <summary>
    /// Overhealth basically is the health above maximum health. 
    /// </summary>
    /// <remarks>Cannot be lower than 0.</remarks>
    public int Overhealth => Math.Max(0, Player.statLife - Player.statLifeMax2);

    public float OverhealthDecreaseRateMultiplier => ModContent.GetInstance<GameplayConfig>().OverhealthDecreaseRateMutiplier;
    public float ConstantDecreaseRate => ModContent.GetInstance<GameplayConfig>().ConstantDecreaseRate;
    public float ProgressiveDecreaseRate => ModContent.GetInstance<GameplayConfig>().ProgressiveDecreaseRate;

    /// <summary>
    /// Amount of overhealth passive decrease divided by 60 per second. The more overhealth, the faster it decreases. 
    /// Constant decrease rate is <see cref="ConstantDecreaseRate"/> of max health per second.
    /// Progressive decrease rate is <see cref="ProgressiveDecreaseRate"/> of max health per second.
    /// </summary>
    /// <see cref="PostUpdateEquips"/>
    /// <see cref="_overhealthDecreaseCounter"/>
    public int OverhealthDecreaseRate => (int)(OverhealthDecreaseRateMultiplier *
        ((Player.statLifeMax2 / 100f * ConstantDecreaseRate) + // Constant decrease rate
        (Overhealth / (float)Player.statLifeMax2 * Player.statLifeMax2 / 100f * ProgressiveDecreaseRate)) // Variable decrease rate based on current overhealth
    );

    private int _overhealthDecreaseCounter = 0;

    /// <summary>
    /// Caps the overhealth at <see cref="Player.statLifeMax2"/>.
    /// </summary>
    public static void CapOverhealth(Player player)
    {
        if (player.GetOverhealth() > player.statLifeMax2)
            player.statLife = player.statLifeMax2 * 2;
    }

    /// <inheritdoc cref="CapOverhealth(Player)"/>
    public void CapOverhealth()
    {
        if (Overhealth > Player.statLifeMax2)
            Player.statLife = Player.statLifeMax2 * 2;
    }

    public override void Load()
    {
        // Replace all health caps with CapOverhealth
        IL_Player.ApplyLifeAndOrMana += ILModify_Player_ApplyLifeAndOrMana;
        IL_Player.Heal += ILModify_Player_Heal;
        IL_Player.UpdateLifeRegen += ILModify_Player_UpdateLifeRegen;
        IL_Player.Update += ILModify_Player_Update;
        // Spectre armor set bonus healing
        IL_Projectile.VanillaAI += ILModify_Projectile_SpectreHealing;
    }

    // Validates overhealth
    public override void PreUpdate()
    {
        CapOverhealth();
    }

    // Gradually decreases overhealth and validates it
    public override void PostUpdate()
    {
        CapOverhealth();
        DecreaseOverhealthTick();
    }

    public void DecreaseOverhealthTick()
    {
        if (Overhealth == 0)
            return;

        _overhealthDecreaseCounter += OverhealthDecreaseRate;

        if (_overhealthDecreaseCounter >= 60)
        {
            int decrease = _overhealthDecreaseCounter / 60;
            _overhealthDecreaseCounter %= 60;

            Player.statLife -= Math.Min(decrease, Overhealth);
        }
    }

    private void ILModify_Player_ApplyLifeAndOrMana(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            c.GotoNext(MoveType.After, i => i.MatchLdfld<Player>("statLife")); // this.statLife += num
            c.GotoNext(MoveType.Before, i => i.MatchLdfld<Player>("statLife")); // if (this.statLife > this.statLifeMax2)
            c.Index--;

            c.RemoveRange(9); // Remove cap chec

            c.EmitLdarg0(); // Load `this` (Player)
            c.EmitCall(GetType().GetMethod(nameof(CapOverhealth), [typeof(Player)]));
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }

    private void ILModify_Player_Heal(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            c.GotoNext(MoveType.After, i => i.MatchCall<Player>(nameof(Player.HealEffect)));
            c.RemoveRange(9); // Remove cap check

            c.EmitLdarg0(); // Load `this` (Player)
            c.EmitCall(GetType().GetMethod(nameof(CapOverhealth), [typeof(Player)]));
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }

    private void ILModify_Player_UpdateLifeRegen(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            c.GotoNext(i => i.MatchLdfld<Player>("lifeRegenCount") && i.Next.MatchLdcI4(120)); // if (this.lifeRegenCount >= 120)
            c.GotoNext(i => i.MatchLdfld<Player>("statLifeMax2") && i.Next.MatchStfld<Player>("statLife")); // this.statLife = this.statLifeMax2;
            c.Index -= 7;

            c.RemoveRange(9); // Remove cap check
            c.EmitLdarg0(); // Load `this` (Player)
            c.EmitCall(GetType().GetMethod(nameof(CapOverhealth), [typeof(Player)]));
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }

    private void ILModify_Player_Update(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            // Right before player health cap at the end of Player.Update
            c.GotoNext(MoveType.After, i => i.MatchCallvirt<Mount>(nameof(Mount.UseDrill)));

            c.RemoveRange(9); // Remove cap check
            c.EmitLdarg0(); // Load `this` (Player)
            c.EmitCall(GetType().GetMethod(nameof(CapOverhealth), [typeof(Player)]));
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }

    private void ILModify_Projectile_SpectreHealing(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            c.GotoNext(i => i.MatchLdfld<Projectile>("aiStyle") && i.Next.MatchLdcI4(52)); // if (this.aiStyle == 52)
            c.GotoNext(MoveType.After, i => i.MatchStfld<Player>("statLife")); // player13.statLife += num418;

            c.RemoveRange(25); // Remove cap
            c.EmitLdloc(643); // Load `player3` (player who is healed)
            c.EmitCall(GetType().GetMethod(nameof(CapOverhealth), [typeof(Player)]));
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }
}