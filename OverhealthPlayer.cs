using System;
using MonoMod.Cil;
using OverhealthMod.Common.Configs;
using OverhealthMod.Utils;
using Terraria;
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
    /// <see cref="DecreaseOverhealthTick"/>
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
        IL_Player.ApplyLifeAndOrMana += CommonIL.ReplaceHealthCapWithCapOverhealth;
        IL_Player.Heal += CommonIL.ReplaceHealthCapWithCapOverhealth;
        IL_Player.UpdateLifeRegen += CommonIL.ReplaceHealthCapWithCapOverhealth;
        IL_Player.Update += CommonIL.ReplaceHealthCapWithCapOverhealth;
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