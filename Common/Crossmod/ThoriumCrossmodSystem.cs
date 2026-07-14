using System;
using System.Reflection;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items.Donate;
using ThoriumMod.Utilities;
using ThoriumMod.Projectiles.Healer;
using ThoriumMod.Projectiles;
using ThoriumMod.Projectiles.Scythe;
using ThoriumMod.Projectiles.Minions;
using ThoriumMod.Items.HealerItems;
using ThoriumMod.Projectiles.Boss;


namespace OverhealthMod.Common.Crossmod;

[ExtendsFromMod("ThoriumMod")]
public class ThoriumCrossmodSystem : ModSystem
{
    public override void Load()
    {
        Assembly thoriumModAssembly = typeof(ThoriumMod.ThoriumMod).Assembly;

        // Core methods
        EditMethod(typeof(PlayerHelper), nameof(PlayerHelper.HealLife), ILModify_PlayerHelper_HealLife); // Remove health cap and set `true` for `healOverMax`
        EditMethod(thoriumModAssembly, "ThoriumMod.Utilities.ProjectileHelper", "CanBeHealed", IL_RemoveHealthCap(i => i.MatchLdarg2()));

        // Weapons
        EditMethod<HereticBreaker>(nameof(HereticBreaker.OnHitNPC), IL_RemoveHealthCap(i => i.MatchLdloc(7)));
        EditMethod<Recuperate>(nameof(Recuperate.CanUseItem), IL_RemoveHealthCap(i => i.MatchLdarg1()));
        EditMethod<HolyHammer>(nameof(HolyHammer.Shoot), IL_RemoveHealthCap(i => i.MatchLdloc3()));
        EditMethod<SmitingHammer>(nameof(SmitingHammer.Shoot), IL_RemoveHealthCap(i => i.MatchLdloc(9)));
        EditMethod<TerrariansLastKnife>(nameof(TerrariansLastKnife.OnHitNPC), IL_RemoveHealthCap(i => i.MatchLdarg1()));

        // Projectiles
        EditMethod<BloodTransfusionPro>(nameof(BloodTransfusionPro.OnHitNPC), IL_RemoveHealthCap(i => i.MatchLdloc0()));
        EditMethod<BloodTransfusionProReturn>(nameof(BloodTransfusionProReturn.AI), IL_RemoveHealthCap(i => i.MatchLdloc1()));
        EditMethod<MorningDewPro>(nameof(MorningDewPro.OnFirstHit), IL_RemoveHealthCap(i => i.MatchLdloc3()));
        EditMethod<BiotechProbe>(nameof(BiotechProbe.AI), IL_RemoveHealthCap(i => i.MatchLdloc(4))); // Biotech armor set
        EditMethod<CelestialWandPro>(nameof(CelestialWandPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(12)));
        EditMethod<CellReconstructorPro>(nameof(CellReconstructorPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(11)));
        EditMethod<ChiLanternPro>(nameof(ChiLanternPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(5)));
        EditMethod<CleansingWaterPouchPro>(nameof(CleansingWaterPouchPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(11)));
        EditMethod<CosmicFluxStaffPro>(nameof(CosmicFluxStaffPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(4)));
        EditMethod<FlanPlatterPro>(nameof(FlanPlatterPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(6)));
        EditMethod<GraveGoodPro>(nameof(GraveGoodPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(4)));
        EditMethod<HealingBeam>(nameof(HealingBeam.AI), IL_RemoveHealthCap(i => i.MatchLdloc(8))); // Divine Suff projectile
        EditMethod<IridescentPro>(nameof(IridescentPro.OnHitNPC), IL_RemoveHealthCap(i => i.MatchLdloc3()));
        EditMethod<LifeDeathPro2>(nameof(LifeDeathPro2.AI), IL_RemoveHealthCap(i => i.MatchLdloc(12)));
        EditMethod<LifeDisperserPro>(nameof(LifeDisperserPro.OnHitNPC), IL_RemoveHealthCap(i => i.MatchLdloc3()));
        EditMethod<LifeOrb>(nameof(LifeOrb.AI), IL_RemoveHealthCap(i => i.MatchLdloc(15))); // Life Essence Apparatus projectile
        EditMethod<LifeSpirit>(nameof(LifeSpirit.AI), IL_RemoveHealthCap(i => i.MatchLdloc0())); // Twinkle minion/projectile
        EditMethod<LifeSurgeHeal>(nameof(LifeSurgeHeal.AI), IL_RemoveHealthCap(i => i.MatchLdloc(12)));
        EditMethod<LifeSurgeHealExtra>(nameof(LifeSurgeHealExtra.AI), IL_RemoveHealthCap(i => i.MatchLdloc(12)));
        // EditMethod<LightBeamPro>(nameof(LightBeamPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(4))); // Light-burst wand projectile - uses nameless delegate
        EditMethod<LilCherubsWandPro>(nameof(LilCherubsWandPro.SafeAI), IL_RemoveHealthCap(i => i.MatchLdloc3()));
        EditMethod<NecroticStaffPro>(nameof(NecroticStaffPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(15)));
        EditMethod<OrbLight>(nameof(OrbLight.AI), IL_RemoveHealthCap(i => i.MatchLdloc(4))); // Twilight Staff projectile
        EditMethod<OrbLight2>(nameof(OrbLight2.AI), IL_RemoveHealthCap(i => i.MatchLdloc0())); // Twilight Staff projectile
        EditMethod<ProphecyPro>(nameof(ProphecyPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(5)));
        EditMethod<PumpkinHealerPro>(nameof(PumpkinHealerPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(9))); // Snack'o'Lantern projectile
        EditMethod<RecuperatePro>(nameof(RecuperatePro.AI), IlModify_RecuperatePro_AI);
        EditMethod<SacredChargeChampion>(nameof(SacredChargeChampion.AI), IL_RemoveHealthCap(i => i.MatchLdloc0()));
        EditMethod<TheGigaNeedlePro>(nameof(TheGigaNeedlePro.AI), IL_RemoveHealthCap(i => i.MatchLdloc3()));
        EditMethod<UnboundFantasyPro>(nameof(UnboundFantasyPro.OnKill), ILModify_UnboundFantasyPro_OnKill);
        EditMethod<ValhallasDescentPro>(nameof(ValhallasDescentPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc2()));
        EditMethod<WallChickenPro>(nameof(WallChickenPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc1())); // On Viscount Rock Fall death

        // Armor
        EditMethod<ThoriumPlayer>(nameof(ThoriumPlayer.OnHurt), IL_RemoveHealthCap(i => i.MatchLdloc(22))); // Fallen Paladin set
        EditMethod<ThoriumProjectileFix>("HealerOnHitNPC", IL_RemoveHealthCap(i => i.MatchLdloc(15))); // Iridescent set

        // Minions
        EditMethod<BannerMoraleHeartPro>(nameof(BannerMoraleHeartPro.AI), IL_RemoveHealthCap(i => i.MatchLdloc(5)));
    }

    private void EditMethod<T>(string methodName, ILContext.Manipulator manipulator) => EditMethod(typeof(T).GetMethod(methodName), manipulator);

    private void EditMethod(Type type, string methodName, ILContext.Manipulator manipulator) => EditMethod(type.GetMethod(methodName), manipulator);

    private void EditMethod(Assembly assembly, string typeFullName, string methodName, ILContext.Manipulator manipulator) => EditMethod(assembly.GetType(typeFullName).GetMethod(methodName), manipulator);

    private void EditMethod(MethodInfo methodInfo, ILContext.Manipulator manipulator)
    {
        if (methodInfo == null)
            return;
        MonoModHooks.Modify(methodInfo, manipulator);
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
