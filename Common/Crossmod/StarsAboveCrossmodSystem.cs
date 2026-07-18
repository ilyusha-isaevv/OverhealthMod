using System;
using System.Reflection;
using MonoMod.Cil;
using OverhealthMod.Utils;
using StarsAbove.Buffs.Magic.SanguineDespair;
using StarsAbove.Buffs.Melee.BurningDesire;
using StarsAbove.Items.Weapons.Melee;
using Terraria;
using Terraria.ModLoader;

namespace OverhealthMod.Common.Crossmod;

[ExtendsFromMod("StarsAbove")]
public class StarsAboveCrossmodSystem : ModSystem
{
    public override void Load()
    {
        // Cap statLife sub to 0
        QuickIL.EditMethod<BoilingBloodBuff>(nameof(BoilingBloodBuff.Update),
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, CapStatLifeSub);
        QuickIL.EditMethod<FeralDespair>(nameof(FeralDespair.Update),
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, CapStatLifeSub);
        QuickIL.EditMethod<BurningDesire>(nameof(BurningDesire.Shoot), CapStatLifeSub);
    }

    private void CapStatLifeSub(ILContext il)
    {
        var c = new ILCursor(il);
        // Move after `statLifeMax2 - statLife`
        c.GotoNext(i => i.MatchLdfld<Player>(nameof(Player.statLifeMax2)) &&
                        i.Next.Next.MatchLdfld<Player>(nameof(Player.statLife)) &&
                        i.Next.Next.Next.MatchSub());
        c.Index += 4;

        // Emit `Math.Max(0, {sub})` to prevent go lower than 0
        c.EmitLdcI4(0);
        c.EmitCall(typeof(Math).GetMethod(nameof(Math.Max), [typeof(int), typeof(int)]));
        MonoModHooks.DumpIL(Mod, il);
    }
}
