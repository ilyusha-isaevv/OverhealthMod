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

    /// <inheritdoc cref="OverhealthPlayer.MaximumOverhealth"/>
    public static int GetMaximumOverhealth(this Player p) =>
        Math.Max(1, (int)(p.statLifeMax2 * OverhealthPlayer.MaximumOverhealthPercent / 100f));
}

public class OverhealthPlayer : ModPlayer
{
    #region Config values
    public static float MaximumOverhealthPercent => ModContent.GetInstance<GameplayConfig>().MaximumOverhealth;
    public static float ConstantDecayPercent => ModContent.GetInstance<GameplayConfig>().ConstantDecayPercent;
    public static float ProgressiveDecayPercent => ModContent.GetInstance<GameplayConfig>().ProgressiveDecayPercent;
    public static float DecayMultiplier => ModContent.GetInstance<GameplayConfig>().DecayMutiplier;
    #endregion

    private int _overhealthDecreaseCounter = 0;

    /// <summary>
    /// Overhealth basically is the health above maximum health. 
    /// </summary>
    /// <remarks>Cannot be lower than 0.</remarks>
    public int Overhealth => Math.Max(0, Player.statLife - Player.statLifeMax2);

    /// <summary>
    /// Maximum <see cref="Overhealth"/> the player can have.
    /// </summary>
    public int MaximumOverhealth => Math.Max(1, (int)(Player.statLifeMax2 * MaximumOverhealthPercent / 100f));

    /// <summary>   
    /// The base value of <see cref="Overhealth"/> passive decrease. <br/>
    /// <b>Raw</b> means it's a <c>float</c> value, while <see cref="Overhealth"/> is an <c>int</c> value.
    /// </summary>
    public float ConstantDecayRaw => (int)(Player.statLifeMax2 * ConstantDecayPercent / 100f);

    /// <summary>
    /// The extra value of <see cref="Overhealth"/> passive decrease that scales with current overhealth.
    /// </summary>
    public float ProgressiveDecayRaw =>
        (float)Overhealth / MaximumOverhealth * MaximumOverhealth * ProgressiveDecayPercent / 100f;

    /// <summary>
    /// Amount of overhealth passive decrease divided by 60 per second.
    /// </summary>
    public float OverhealthDecayRaw => DecayMultiplier * (ConstantDecayRaw + ProgressiveDecayRaw);

    /// <summary>
    /// Post-processed value of <see cref="OverhealthDecayRaw"/>.
    /// </summary>
    /// <remarks>
    /// Always greater than 0.
    /// </remarks>
    /// <see cref="DecreaseOverhealthTick"/>
    public int OverhealthDecay => Math.Max(1, (int)Math.Round(OverhealthDecayRaw));

    /// <summary>
    /// Caps the overhealth at <see cref="Player.statLifeMax2"/>.
    /// </summary>
    public static void CapOverhealth(Player player)
    {
        if (player.GetOverhealth() > player.GetMaximumOverhealth())
            player.statLife = player.statLifeMax2 + player.GetMaximumOverhealth();
    }

    /// <inheritdoc cref="CapOverhealth(Player)"/>
    public void CapOverhealth()
    {
        if (Overhealth > MaximumOverhealth)
            Player.statLife = Player.statLifeMax2 + MaximumOverhealth;
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

        _overhealthDecreaseCounter += OverhealthDecay;

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