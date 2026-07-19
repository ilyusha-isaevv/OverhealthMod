using System;
using System.Reflection;
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
        QuickIL.EditMethod(thoriumModAssembly, "ThoriumMod.Utilities.ProjectileHelper", "CanBeHealed", CommonIL.RemoveHealthCapCheck_Bge);

        // Weapons
        QuickIL.EditMethod<HereticBreaker>(nameof(HereticBreaker.OnHitNPC), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<Recuperate>(nameof(Recuperate.CanUseItem), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<HolyHammer>(nameof(HolyHammer.Shoot), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<SmitingHammer>(nameof(SmitingHammer.Shoot), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<TerrariansLastKnife>(nameof(TerrariansLastKnife.OnHitNPC), CommonIL.RemoveHealthCapCheck_Bge);

        // // Projectiles
        QuickIL.EditMethod<BloodTransfusionPro>(nameof(BloodTransfusionPro.OnHitNPC), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<BloodTransfusionProReturn>(nameof(BloodTransfusionProReturn.AI), CommonIL.RemoveHealthCapCheck_Blt);
        QuickIL.EditMethod<MorningDewPro>(nameof(MorningDewPro.OnFirstHit), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<BiotechProbe>(nameof(BiotechProbe.AI), CommonIL.RemoveHealthCapCheck_Bge); // Biotech armor set
        QuickIL.EditMethod<CelestialWandPro>(nameof(CelestialWandPro.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<CellReconstructorPro>(nameof(CellReconstructorPro.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<ChiLanternPro>(nameof(ChiLanternPro.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<CleansingWaterPouchPro>(nameof(CleansingWaterPouchPro.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<CosmicFluxStaffPro>(nameof(CosmicFluxStaffPro.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<FlanPlatterPro>(nameof(FlanPlatterPro.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<GraveGoodPro>(nameof(GraveGoodPro.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<HealingBeam>(nameof(HealingBeam.AI), CommonIL.RemoveHealthCapCheck_Bge); // Divine Suff projectile
        QuickIL.EditMethod<IridescentPro>(nameof(IridescentPro.OnHitNPC), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<LifeDeathPro2>(nameof(LifeDeathPro2.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<LifeDisperserPro>(nameof(LifeDisperserPro.OnHitNPC), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<LifeOrb>(nameof(LifeOrb.AI), CommonIL.RemoveHealthCapCheck_Bge); // Life Essence Apparatus projectile
        QuickIL.EditMethod<LifeSpirit>(nameof(LifeSpirit.AI), CommonIL.RemoveHealthCapCheck_Bge); // Twinkle minion/projectile
        QuickIL.EditMethod<LifeSurgeHeal>(nameof(LifeSurgeHeal.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<LifeSurgeHealExtra>(nameof(LifeSurgeHealExtra.AI), CommonIL.RemoveHealthCapCheck_Bge);
        // QuickIL.EditMethod<LightBeamPro>(nameof(LightBeamPro.AI), CommonIL.RemoveHealthCapIL(i => i.MatchLdloc(4))); // Light-burst wand projectile - uses nameless delegate
        QuickIL.EditMethod<LilCherubsWandPro>(nameof(LilCherubsWandPro.SafeAI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<NecroticStaffPro>(nameof(NecroticStaffPro.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<OrbLight>(nameof(OrbLight.AI), CommonIL.RemoveHealthCapCheck_Bge); // Twilight Staff projectile
        QuickIL.EditMethod<OrbLight2>(nameof(OrbLight2.AI), CommonIL.RemoveHealthCapCheck_Bge); // Twilight Staff projectile
        QuickIL.EditMethod<ProphecyPro>(nameof(ProphecyPro.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<PumpkinHealerPro>(nameof(PumpkinHealerPro.AI), CommonIL.RemoveHealthCapCheck_Bge); // Snack'o'Lantern projectile
        QuickIL.EditMethod<RecuperatePro>(nameof(RecuperatePro.AI), IlModify_RecuperatePro_AI);
        QuickIL.EditMethod<SacredChargeChampion>(nameof(SacredChargeChampion.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<TheGigaNeedlePro>(nameof(TheGigaNeedlePro.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<UnboundFantasyPro>(nameof(UnboundFantasyPro.OnKill), ILModify_UnboundFantasyPro_OnKill);
        QuickIL.EditMethod<ValhallasDescentPro>(nameof(ValhallasDescentPro.AI), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<WallChickenPro>(nameof(WallChickenPro.AI), CommonIL.RemoveHealthCapCheck_Bge); // On Viscount Rock Fall death

        // Armor
        QuickIL.EditMethod<ThoriumPlayer>(nameof(ThoriumPlayer.OnHurt), CommonIL.RemoveHealthCapCheck_Bge); // Fallen Paladin set
        QuickIL.EditMethod<ThoriumProjectileFix>("HealerOnHitNPC",
            BindingFlags.NonPublic | BindingFlags.Instance, CommonIL.RemoveHealthCapCheck_Bge); // Iridescent set

        // Minions
        QuickIL.EditMethod<BannerMoraleHeartPro>(nameof(BannerMoraleHeartPro.AI), CommonIL.RemoveHealthCapCheck_Bge);
    }

    private void ILModify_PlayerHelper_HealLife(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            c.EmitLdcI4(1); // Load `true`
            c.EmitStarg(3); // Always set `healOverMax` to `true`

            c.GotoHealthCap();
            c.Index -= 2; // Move before `if (!healOverMax && ...)`

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
            for (int i = 0; i < 3; i++)
            {
                c.GotoHealthCapCheck_Bge();
                c.RemoveRange(5);
            }
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
            c.GotoHealthCapCheck_Blt();
            c.GotoPrev(MoveType.After, i => i.MatchStloc1());
            c.RemoveRange(14); // Remove whole life check and projectile kill logic
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }
}
