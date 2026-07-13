using System;
using System.IO;
using MonoMod.Cil;
using OverhealthMod.Common.Configs;
using Terraria;
using Terraria.ModLoader;

namespace OverhealthMod;

public class OverhealthPlayer : ModPlayer
{
    public float OverhealthDecreaseRateMultiplier => ModContent.GetInstance<GameplayConfig>().OverhealthDecreaseRateMutiplier;
    public float ConstantDecreaseRate => ModContent.GetInstance<GameplayConfig>().ConstantDecreaseRate;
    public float ProgressiveDecreaseRate => ModContent.GetInstance<GameplayConfig>().ProgressiveDecreaseRate;

    /// <summary>
    /// Amount of overhealth passive decrease divided by 60 per second. Depends on current overhealth. 
    /// The more overhealth, the faster it decreases. Default constant decrease rate is <see cref="ConstantDecreaseRate"/> of max health per second.
    /// Default progressive decrease rate is <see cref="ProgressiveDecreaseRate"/> of max health per second.
    /// At 100% overhealth it decreases at <see cref="ConstantDecreaseRate"/> + <see cref="ProgressiveDecreaseRate"/> of max health per second.
    /// Can be adjusted by <see cref="OverhealthDecreaseRateMultiplier"/> config value.
    /// </summary>
    /// <see cref="PostUpdateEquips"/>
    /// <see cref="_overhealthDecreaseCounter"/>
    public int OverhealthDecreaseRate => (int)(OverhealthDecreaseRateMultiplier *
        ((Player.statLifeMax2 / 100f * ConstantDecreaseRate) + // Constant decrease rate
        (Overhealth / (float)Player.statLifeMax2 * Player.statLifeMax2 / 100f * ProgressiveDecreaseRate)) // Variable decrease rate based on current overhealth
    );

    private int _overhealthDecreaseCounter = 0;

    private int _overhealth = 0;
    public int Overhealth
    {
        get => _overhealth;
        internal set
        {
            if (Player.creativeGodMode)
                _overhealth = 0;
            else
                _overhealth = Math.Clamp(value, 0, Player.statLifeMax2);

            if (Overhealth == 0)
                _overhealthDecreaseCounter = 0;
        }

    }

    /// <summary>
    /// Checks if during the last tick the <paramref name="player"/> has lost overhealth and updates the <see cref="Overhealth"/> value accordingly.
    /// </summary>
    public static void ValidateOverhealth(Player player)
    {
        OverhealthPlayer overhealthPlayer = player.GetModPlayer<OverhealthPlayer>();
        if (overhealthPlayer.Overhealth == 0)
            return;

        int currentOverhealth = player.statLife - player.statLifeMax2;
        if (currentOverhealth < overhealthPlayer.Overhealth)
            overhealthPlayer.Overhealth = currentOverhealth;
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)OverhealthMod.MessageType.SyncOverhealth);
        packet.Write((byte)Player.whoAmI);
        packet.Write(Overhealth);
        packet.Send(toWho, fromWho);
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        OverhealthPlayer clone = (OverhealthPlayer)targetCopy;
        clone._overhealth = _overhealth;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        OverhealthPlayer clone = (OverhealthPlayer)clientPlayer;
        if (clone._overhealth != _overhealth)
            SyncPlayer(-1, Main.myPlayer, false);
    }

    public void RecieveSyncPlayer(BinaryReader reader)
    {
        Overhealth = reader.ReadInt32();
    }

    /// <summary>
    /// Transfers overheal amount to <see cref="Overhealth"/>.
    /// Apply <see cref="ValidateOverhealth(Player)"/> before calling this method to ensure the <paramref name="player"/> has not lost overhealth during the last tick. 
    /// </summary>
    /// <remarks>This method do not heals the <paramref name="player"/>.</remarks>
    public static void OverhealToOverhealth(Player player, int healAmount)
    {
        OverhealthPlayer overhealthPlayer = player.GetModPlayer<OverhealthPlayer>();

        int healLeftToOverheal = Math.Max(0, player.statLifeMax2 - player.statLife);
        int overheal = Math.Max(0, healAmount - healLeftToOverheal);
        overhealthPlayer.Overhealth += overheal;
    }

    /// <inheritdoc cref="ApplyOverhealth(OverhealthPlayer)"/>
    public static void ApplyOverhealth(Player player) => ApplyOverhealth(player.GetModPlayer<OverhealthPlayer>());

    /// <summary>
    /// Applies the <paramref name="overhealthPlayer"/>'s <see cref="Overhealth"/> to the <see cref="Player.statLife"/>.
    /// </summary>
    /// <param name="overhealthPlayer"></param>
    public static void ApplyOverhealth(OverhealthPlayer overhealthPlayer)
    {
        if (overhealthPlayer.Overhealth == 0) return;

        overhealthPlayer.Player.statLife = overhealthPlayer.Player.statLifeMax2 + overhealthPlayer.Overhealth;
    }

    public override void Load()
    {
        // Heal methods
        On_Player.Heal += OnPlayerHeal;
        On_Player.ApplyLifeAndOrMana += OnPlayerApplyLifeAndOrMana;
        IL_Player.UpdateLifeRegen += ILModify_Player_UpdateLifeRegen;
        IL_Player.ApplyLifeAndOrMana += ILModify_Player_ApplyLifeAndOrMana;
        // Spectre armor set bonus healing
        IL_Projectile.VanillaAI += ILModify_Projectile_SpectreHealing;

        IL_Player.Update += ILModify_Player_OverhealthCheck;
    }

    public override void PreUpdate()
    {
        ValidateOverhealth(Player);
    }

    // Gradually decrease overhealth and 
    // Apply overhealth to player after bound reset at the end of Player.Update
    public override void PostUpdate()
    {
        if (Player.creativeGodMode) return;
        if (Overhealth == 0) return;

        DecreaseOverhealthTick();
        ApplyOverhealth(this);
    }

    public void DecreaseOverhealthTick()
    {
        _overhealthDecreaseCounter += OverhealthDecreaseRate;

        if (_overhealthDecreaseCounter >= 60)
        {
            int decrease = _overhealthDecreaseCounter / 60;
            _overhealthDecreaseCounter %= 60;
            Overhealth -= decrease;
        }
    }

    private void OnPlayerHeal(On_Player.orig_Heal orig, Player player, int healAmount)
    {
        if (Main.myPlayer != player.whoAmI)
        {
            orig(player, healAmount);
            return;
        }

        ValidateOverhealth(player);
        OverhealToOverhealth(player, healAmount);
        orig(player, healAmount);
        ApplyOverhealth(player);
    }

    private void OnPlayerApplyLifeAndOrMana(On_Player.orig_ApplyLifeAndOrMana orig, Player player, Item item)
    {
        orig(player, item);
        ApplyOverhealth(player); // Apply overhealth after life bound reset
    }

    private void ILModify_Player_UpdateLifeRegen(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            c.GotoNext(i => i.MatchLdfld<Player>("lifeRegenCount") && i.Next.MatchLdcI4(120)); // if (this.lifeRegenCount >= 120)
            c.GotoNext(i => i.MatchLdfld<Player>("statLifeMax2") && i.Next.MatchStfld<Player>("statLife")); // this.statLife = this.statLifeMax2;

            c.EmitLdarg0(); // Load `this` (Player)
            c.EmitCall(typeof(OverhealthPlayer).GetMethod("ValidateOverhealth", [typeof(Player)])); // Validate overhealth before life bound reset

            c.Index += 2; // Move after statLife assignment
            c.EmitLdarg0(); // Load `this` (Player)
            c.EmitCall(typeof(OverhealthPlayer).GetMethod("ApplyOverhealth", [typeof(Player)])); // Apply overhealth after life bound reset
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }

    private void ILModify_Player_ApplyLifeAndOrMana(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            // this.statLife += num; 72 index
            // on this opcode `num` (heal amount) is already calculated
            c.GotoNext(i => i.MatchLdfld<Player>("statLife"));

            c.EmitLdarg0(); // Load `this` (Player)
            c.EmitCall(typeof(OverhealthPlayer).GetMethod("ValidateOverhealth", [typeof(Player)])); // Validate overhealth before life bound reset

            c.EmitLdarg0(); // Load `this` (Player)
            c.EmitLdloc0(); // Load first local variable: num (heal amount)
            c.EmitCall(typeof(OverhealthPlayer).GetMethod("OverhealToOverhealth", [typeof(Player), typeof(int)])); // Convert overheal to overhealth

            c.GotoNext(i => i.MatchLdfld<Player>("statLifeMax2") && i.Next.MatchStfld<Player>("statLife")); // this.statLife = this.statLifeMax2;"
            c.Index += 2; // Move after statLife assignment
            c.EmitLdarg0(); // Load `this` (Player)
            c.EmitCall(typeof(OverhealthPlayer).GetMethod("ApplyOverhealth", [typeof(Player)])); // Apply overhealth after life bound reset
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
            c.GotoNext(i => i.MatchLdfld<Player>("statLife")); // player13.statLife

            c.EmitLdloc(644); // player13 (Player who is healed)
            c.EmitCall(typeof(OverhealthPlayer).GetMethod("ValidateOverhealth", [typeof(Player)])); // Validate overhealth before life bound reset

            c.EmitLdloc(644); // player13 (Player who is healed)
            c.EmitLdloc(642); // Load local variable: num (heal amount)
            c.EmitCall(typeof(OverhealthPlayer).GetMethod("OverhealToOverhealth", [typeof(Player), typeof(int)])); // Convert overheal to overhealth

            // Bypass max health bound check
            c.GotoNext(i => i.MatchLdfld<Player>("statLifeMax2") && i.Next.MatchStfld<Player>("statLife")); // this.statLife = this.statLifeMax2;"
            c.Index += 2; // Move after statLife assignment
            c.EmitLdloc(644); // player13 (Player who is healed)
            c.EmitCall(typeof(OverhealthPlayer).GetMethod("ApplyOverhealth", [typeof(Player)])); // Apply overhealth after life bound reset
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }

    private void ILModify_Player_OverhealthCheck(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            // Right before player health bound check at the end of Player.Update
            c.GotoNext(i => i.MatchCall<Player>("ItemCheckWrapped")); // this.ItemCheckWrapped(i)
            c.Index += 3; // After ItemCheckWrapped() and PlayerFrame()

            c.EmitLdarg0(); // Load `this` (Player)
            c.EmitCall(typeof(OverhealthPlayer).GetMethod("ValidateOverhealth", [typeof(Player)])); // Apply overhealth after life bound reset
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }
}