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
        Assembly thoriumModAssembly = typeof(ThoriumMod.ThoriumMod).Assembly;

        // Core methods
        QuickIL.EditMethod(typeof(PlayerHelper), nameof(PlayerHelper.HealLife), ILModify_PlayerHelper_HealLife); // Remove health cap and set `true` for `healOverMax`
        QuickIL.EditMethod(thoriumModAssembly, "ThoriumMod.Utilities.ProjectileHelper", "CanBeHealed", IL_RemoveHealthCap(i => i.MatchLdarg2()));

        // Weapons
        QuickIL.EditMethod<HereticBreaker>(nameof(HereticBreaker.OnHitNPC), IL_RemoveHealthCap(i => i.MatchLdloc(7)));
        QuickIL.EditMethod<Recuperate>(nameof(Recuperate.CanUseItem), IL_RemoveHealthCap(i => i.MatchLdarg1()));
        QuickIL.EditMethod<HolyHammer>(nameof(HolyHammer.Shoot), IL_RemoveHealthCap(i => i.MatchLdloc3()));
        QuickIL.EditMethod<SmitingHammer>(nameof(SmitingHammer.Shoot), IL_RemoveHealthCap(i => i.MatchLdloc(9)));
        QuickIL.EditMethod<TerrariansLastKnife>(nameof(TerrariansLastKnife.OnHitNPC), IL_RemoveHealthCap(i => i.MatchLdarg1()));

        // Projectiles
        QuickIL.EditMethod<BloodTransfusionPro>(nameof(BloodTransfusionPro.OnHitNPC), IL_RemoveHealthCap(i => i.MatchLdloc0()));
        QuickIL.EditMethod<BloodTransfusionProReturn>(nameof(BloodTransfusionProReturn.AI), IL_RemoveHealthCap(i => i.MatchLdloc1()));
        QuickIL.EditMethod<MorningDewPro>(nameof(MorningDewPro.OnFirstHit), IL_RemoveHealthCap(i => i.MatchLdloc3()));
        QuickIL.EditMethod<BiotechProbe>(nameof(BiotechProbe.AI), IL_RemoveHealthCap(i => i.MatchLdloc(4))); // Biotech armor set
        QuickIL.EditMethod<CelestialWandPro>(nameof(CelestialWandPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(12)));
        QuickIL.EditMethod<CellReconstructorPro>(nameof(CellReconstructorPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(11)));
        QuickIL.EditMethod<ChiLanternPro>(nameof(ChiLanternPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(5)));
        QuickIL.EditMethod<CleansingWaterPouchPro>(nameof(CleansingWaterPouchPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(11)));
        QuickIL.EditMethod<CosmicFluxStaffPro>(nameof(CosmicFluxStaffPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(4)));
        QuickIL.EditMethod<FlanPlatterPro>(nameof(FlanPlatterPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(6)));
        QuickIL.EditMethod<GraveGoodPro>(nameof(GraveGoodPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(4)));
        QuickIL.EditMethod<HealingBeam>(nameof(HealingBeam.AI), IL_RemoveHealthCap(i => i.MatchLdloc(8))); // Divine Suff projectile
        QuickIL.EditMethod<IridescentPro>(nameof(IridescentPro.OnHitNPC), IL_RemoveHealthCap(i => i.MatchLdloc3()));
        QuickIL.EditMethod<LifeDeathPro2>(nameof(LifeDeathPro2.AI), IL_RemoveHealthCap(i => i.MatchLdloc(12)));
        QuickIL.EditMethod<LifeDisperserPro>(nameof(LifeDisperserPro.OnHitNPC), IL_RemoveHealthCap(i => i.MatchLdloc3()));
        QuickIL.EditMethod<LifeOrb>(nameof(LifeOrb.AI), IL_RemoveHealthCap(i => i.MatchLdloc(15))); // Life Essence Apparatus projectile
        QuickIL.EditMethod<LifeSpirit>(nameof(LifeSpirit.AI), IL_RemoveHealthCap(i => i.MatchLdloc0())); // Twinkle minion/projectile
        QuickIL.EditMethod<LifeSurgeHeal>(nameof(LifeSurgeHeal.AI), IL_RemoveHealthCap(i => i.MatchLdloc(12)));
        QuickIL.EditMethod<LifeSurgeHealExtra>(nameof(LifeSurgeHealExtra.AI), IL_RemoveHealthCap(i => i.MatchLdloc(12)));
        // QuickIL.EditMethod<LightBeamPro>(nameof(LightBeamPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(4))); // Light-burst wand projectile - uses nameless delegate
        QuickIL.EditMethod<LilCherubsWandPro>(nameof(LilCherubsWandPro.SafeAI), IL_RemoveHealthCap(i => i.MatchLdloc3()));
        QuickIL.EditMethod<NecroticStaffPro>(nameof(NecroticStaffPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(15)));
        QuickIL.EditMethod<OrbLight>(nameof(OrbLight.AI), IL_RemoveHealthCap(i => i.MatchLdloc(4))); // Twilight Staff projectile
        QuickIL.EditMethod<OrbLight2>(nameof(OrbLight2.AI), IL_RemoveHealthCap(i => i.MatchLdloc0())); // Twilight Staff projectile
        QuickIL.EditMethod<ProphecyPro>(nameof(ProphecyPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(5)));
        QuickIL.EditMethod<PumpkinHealerPro>(nameof(PumpkinHealerPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(9))); // Snack'o'Lantern projectile
        QuickIL.EditMethod<RecuperatePro>(nameof(RecuperatePro.AI), IlModify_RecuperatePro_AI);
        QuickIL.EditMethod<SacredChargeChampion>(nameof(SacredChargeChampion.AI), IL_RemoveHealthCap(i => i.MatchLdloc0()));
        QuickIL.EditMethod<TheGigaNeedlePro>(nameof(TheGigaNeedlePro.AI), IL_RemoveHealthCap(i => i.MatchLdloc3()));
        QuickIL.EditMethod<UnboundFantasyPro>(nameof(UnboundFantasyPro.OnKill), ILModify_UnboundFantasyPro_OnKill);
        QuickIL.EditMethod<ValhallasDescentPro>(nameof(ValhallasDescentPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc2()));
        QuickIL.EditMethod<WallChickenPro>(nameof(WallChickenPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc1())); // On Viscount Rock Fall death

        // Armor
        QuickIL.EditMethod<ThoriumPlayer>(nameof(ThoriumPlayer.OnHurt), IL_RemoveHealthCap(i => i.MatchLdloc(22))); // Fallen Paladin set
        QuickIL.EditMethod<ThoriumProjectileFix>("HealerOnHitNPC", IL_RemoveHealthCap(i => i.MatchLdloc(15))); // Iridescent set

        // Minions
        QuickIL.EditMethod<BannerMoraleHeartPro>(nameof(BannerMoraleHeartPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(5)));
    }

    private void ILCursor_RemoveHealthCap(ILCursor c, Predicate<Mono.Cecil.Cil.Instruction> LoadPlayerMatch)
    {
        c.GotoNext(MoveType.Before,
            i => LoadPlayerMatch(i) && i.Next.MatchLdfld<Player>(nameof(Player.statLife)) &&
            LoadPlayerMatch(i.Next.Next) && i.Next.Next.Next.MatchLdfld<Player>(nameof(Player.statLifeMax2))
        ); // if (... && player.statLife (<=?|>=?|==) player.statLifeMax2 && ...)
        c.RemoveRange(5); // Remove health cap check
    }

    private ILContext.Manipulator IL_RemoveHealthCap(Predicate<Mono.Cecil.Cil.Instruction> LoadPlayerMatch)
    {
        return il =>
        {
            try
            {
                ILCursor c = new(il);
                ILCursor_RemoveHealthCap(c, LoadPlayerMatch);
            }
            catch (Exception)
            {
                MonoModHooks.DumpIL(Mod, il);
            }
        };
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
            ILCursor_RemoveHealthCap(c, i => i.MatchLdloc(8));
            ILCursor_RemoveHealthCap(c, i => i.MatchLdloc(15));
            ILCursor_RemoveHealthCap(c, i => i.MatchLdloc(22));
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
