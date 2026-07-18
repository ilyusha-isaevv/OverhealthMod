using System.Reflection;
using CalamityMod;
using CalamityMod.Enums;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using MonoMod.Cil;
using OverhealthMod.Utils;
using Terraria;
using Terraria.ModLoader;

namespace OverhealthMod.Common.Crossmod;

[ExtendsFromMod("CalamityMod")]
public class CalamityCrossmodSystem : ModSystem
{
    private delegate void orig_HealPlayer(Player player, int amount, HealTextType healTextType);

    public override void Load()
    {
        Assembly calamityModAssembly = ModLoader.GetMod("CalamityMod").Code;

        // Core methods
        QuickIL.EditMethod(typeof(CalamityUtils), nameof(CalamityUtils.ConsumeItemViaQuickBuff), CommonIL.ReplaceHealthCapWithCapOverhealth);
        QuickIL.EditMethod(typeof(CalamityUtils), nameof(CalamityUtils.HealPlayer), CommonIL.ReplaceHealthCapWithCapOverhealth);

        // NPCs - Heart drops
        QuickIL.EditMethod<BrimstoneHeart>(nameof(BrimstoneHeart.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<SoulSeekerSupreme>(nameof(SoulSeekerSupreme.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<CorruptSlimeSpawn2>(nameof(CorruptSlimeSpawn2.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<CrimsonSlimeSpawn>(nameof(CrimsonSlimeSpawn.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<CrimsonSlimeSpawn2>(nameof(CrimsonSlimeSpawn2.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<PerforatorBodyMedium>(nameof(PerforatorBodyMedium.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<PerforatorHeadMedium>(nameof(PerforatorHeadMedium.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<PerforatorTailMedium>(nameof(PerforatorTailMedium.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<OldDukeToothBall>(nameof(OldDukeToothBall.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<SulphurousSharkron>(nameof(SulphurousSharkron.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<AquaticAberration>(nameof(AquaticAberration.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<DankCreeper>(nameof(DankCreeper.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<DarkHeart>(nameof(DarkHeart.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<HiveBlob>(nameof(HiveBlob.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<CrabShroom>(nameof(CrabShroom.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<SoulSeeker>(nameof(SoulSeeker.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<DraconicSwarmer>(nameof(DraconicSwarmer.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<Brimling>(nameof(Brimling.OnKill), CommonIL.RemoveHealthCapCheck_Array);
        QuickIL.EditMethod<AureusSpawn>(nameof(AureusSpawn.OnKill), CommonIL.RemoveHealthCapCheck_Array);

        // Summons
        QuickIL.EditMethod<SandElementalHealer>(nameof(SandElementalHealer.AI), CommonIL.RemoveHealthCapCheck_Bge);

        // Inline. For some reason HealPlayer method inside these methods gets inlined, making it work as default unpatched version of HealPlayer.
        // Any IL patch will make this method un-inlined, so HealPlayer will be called via Call OpCode.
        QuickIL.EditMethod(calamityModAssembly, "CalamityMod.Projectiles.CommonProjectileAI", "HealingProjectile", DisableInline);
        QuickIL.EditMethod<RelicOfConvergenceCrystal>(nameof(RelicOfConvergenceCrystal.AI), DisableInline);
        QuickIL.EditMethod<BlazingStarHeal>(nameof(BlazingStarHeal.AI), DisableInline);
    }

    private void DisableInline(ILContext il)
    {
        ILCursor _ = new(il);
    }
}
