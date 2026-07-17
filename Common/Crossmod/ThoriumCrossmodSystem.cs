using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using OverhealthMod.Utils;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items.Donate;
using ThoriumMod.Items.HealerItems;
using ThoriumMod.Projectiles;
using ThoriumMod.Projectiles.Boss;
using ThoriumMod.Projectiles.Healer;
using ThoriumMod.Projectiles.Minions;
using ThoriumMod.Projectiles.Scythe;
using ThoriumMod.Utilities;

namespace OverhealthMod.Common.Crossmod;

[ExtendsFromMod("ThoriumMod")]
public class ThoriumCrossmodSystem : ModSystem
{
    public override void Load()
    {
        Assembly thoriumModAssembly = ModLoader.GetMod("ThoriumMod").Code;

        // Core methods
        QuickIL.EditMethod(typeof(PlayerHelper), nameof(PlayerHelper.HealLife), ILModify_PlayerHelper_HealLife); // Remove health cap and set `true` for `healOverMax`
        QuickIL.EditMethod(thoriumModAssembly, "ThoriumMod.Utilities.ProjectileHelper", "CanBeHealed", CommonIL.RemoveHealthCapCheck(OpCodes.Ldarg_2));

        // Weapons
        QuickIL.EditMethod<HereticBreaker>(nameof(HereticBreaker.OnHitNPC), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_S, 7));
        QuickIL.EditMethod<Recuperate>(nameof(Recuperate.CanUseItem), CommonIL.RemoveHealthCapCheck(OpCodes.Ldarg_1));
        QuickIL.EditMethod<HolyHammer>(nameof(HolyHammer.Shoot), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_3));
        QuickIL.EditMethod<SmitingHammer>(nameof(SmitingHammer.Shoot), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 9));
        QuickIL.EditMethod<TerrariansLastKnife>(nameof(TerrariansLastKnife.OnHitNPC), CommonIL.RemoveHealthCapCheck(OpCodes.Ldarg_1));

        // Projectiles
        QuickIL.EditMethod<BloodTransfusionPro>(nameof(BloodTransfusionPro.OnHitNPC), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_0));
        QuickIL.EditMethod<BloodTransfusionProReturn>(nameof(BloodTransfusionProReturn.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_1));
        QuickIL.EditMethod<MorningDewPro>(nameof(MorningDewPro.OnFirstHit), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_3));
        QuickIL.EditMethod<BiotechProbe>(nameof(BiotechProbe.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 4)); // Biotech armor set
        QuickIL.EditMethod<CelestialWandPro>(nameof(CelestialWandPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 12));
        QuickIL.EditMethod<CellReconstructorPro>(nameof(CellReconstructorPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 11));
        QuickIL.EditMethod<ChiLanternPro>(nameof(ChiLanternPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 5));
        QuickIL.EditMethod<CleansingWaterPouchPro>(nameof(CleansingWaterPouchPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 11));
        QuickIL.EditMethod<CosmicFluxStaffPro>(nameof(CosmicFluxStaffPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 4));
        QuickIL.EditMethod<FlanPlatterPro>(nameof(FlanPlatterPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 6));
        QuickIL.EditMethod<GraveGoodPro>(nameof(GraveGoodPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 4));
        QuickIL.EditMethod<HealingBeam>(nameof(HealingBeam.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 8)); // Divine Suff projectile
        QuickIL.EditMethod<IridescentPro>(nameof(IridescentPro.OnHitNPC), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_3));
        QuickIL.EditMethod<LifeDeathPro2>(nameof(LifeDeathPro2.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 12));
        QuickIL.EditMethod<LifeDisperserPro>(nameof(LifeDisperserPro.OnHitNPC), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_3));
        QuickIL.EditMethod<LifeOrb>(nameof(LifeOrb.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 15)); // Life Essence Apparatus projectile
        QuickIL.EditMethod<LifeSpirit>(nameof(LifeSpirit.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_0)); // Twinkle minion/projectile
        QuickIL.EditMethod<LifeSurgeHeal>(nameof(LifeSurgeHeal.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 12));
        QuickIL.EditMethod<LifeSurgeHealExtra>(nameof(LifeSurgeHealExtra.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 12));
        // QuickIL.EditMethod<LightBeamPro>(nameof(LightBeamPro.AI), CommonIL.RemoveHealthCapIL(i => i.MatchLdloc(4))); // Light-burst wand projectile - uses nameless delegate
        QuickIL.EditMethod<LilCherubsWandPro>(nameof(LilCherubsWandPro.SafeAI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_3));
        QuickIL.EditMethod<NecroticStaffPro>(nameof(NecroticStaffPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 15));
        QuickIL.EditMethod<OrbLight>(nameof(OrbLight.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 4)); // Twilight Staff projectile
        QuickIL.EditMethod<OrbLight2>(nameof(OrbLight2.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_0)); // Twilight Staff projectile
        QuickIL.EditMethod<ProphecyPro>(nameof(ProphecyPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 5));
        QuickIL.EditMethod<PumpkinHealerPro>(nameof(PumpkinHealerPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 9)); // Snack'o'Lantern projectile
        QuickIL.EditMethod<RecuperatePro>(nameof(RecuperatePro.AI), IlModify_RecuperatePro_AI);
        QuickIL.EditMethod<SacredChargeChampion>(nameof(SacredChargeChampion.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_0));
        QuickIL.EditMethod<TheGigaNeedlePro>(nameof(TheGigaNeedlePro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_3));
        QuickIL.EditMethod<UnboundFantasyPro>(nameof(UnboundFantasyPro.OnKill), ILModify_UnboundFantasyPro_OnKill);
        QuickIL.EditMethod<ValhallasDescentPro>(nameof(ValhallasDescentPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_2));
        QuickIL.EditMethod<WallChickenPro>(nameof(WallChickenPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc_1)); // On Viscount Rock Fall death

        // Armor
        QuickIL.EditMethod<ThoriumPlayer>(nameof(ThoriumPlayer.OnHurt), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 22)); // Fallen Paladin set
        QuickIL.EditMethod<ThoriumProjectileFix>("HealerOnHitNPC", CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 15)); // Iridescent set

        // Minions
        QuickIL.EditMethod<BannerMoraleHeartPro>(nameof(BannerMoraleHeartPro.AI), CommonIL.RemoveHealthCapCheck(OpCodes.Ldloc, 5));
    }

    private void ILModify_PlayerHelper_HealLife(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            c.EmitLdcI4(1); // Load `true`
            c.EmitStarg(3); // Always set `healOverMax` to `true`

            c.GotoNext(MoveType.After, i => i.MatchCallvirt<ThoriumPlayer>(nameof(ThoriumPlayer.AddHPS)));
            c.GotoNext(MoveType.Before, i => i.MatchLdarg3());

            c.RemoveRange(11); // Remove health cap
            c.EmitLdarg0(); // Load Player
            c.EmitCall(typeof(OverhealthPlayer).GetMethod(nameof(OverhealthPlayer.CapOverhealth), [typeof(Player)]));
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }

    private void ILModify_UnboundFantasyPro_OnKill(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            c.GotoAndRemoveHealthCapCheck(OpCodes.Ldloc, 8);
            c.GotoAndRemoveHealthCapCheck(OpCodes.Ldloc, 15);
            c.GotoAndRemoveHealthCapCheck(OpCodes.Ldloc, 22);
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }

    private void IlModify_RecuperatePro_AI(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            c.GotoNext(MoveType.After, i => i.MatchLdfld<ThoriumPlayer>(nameof(ThoriumPlayer.healBonus)) && i.Next.MatchStloc1());
            c.Index++;

            c.RemoveRange(14); // Remove whole life check and projectile kill logic
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }
}
