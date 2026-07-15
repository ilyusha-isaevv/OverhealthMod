using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace OverhealthMod.Utils;

/// <summary>
/// Contains common IL edits.
/// </summary>
public static class CommonIL
{
    public static void RemoveHealthCapCheck(this ILCursor c, Predicate<Mono.Cecil.Cil.Instruction> LoadPlayerMatch)
    {
        c.GotoNext(MoveType.Before,
            i => LoadPlayerMatch(i) && i.Next.MatchLdfld<Player>(nameof(Player.statLife)) &&
            LoadPlayerMatch(i.Next.Next) && i.Next.Next.Next.MatchLdfld<Player>(nameof(Player.statLifeMax2))
        ); // ... player.statLife ?? player.statLifeMax2 ...
        c.RemoveRange(5); // Remove health cap check
    }

    public static ILContext.Manipulator RemoveHealthCapCheck(Predicate<Mono.Cecil.Cil.Instruction> LoadPlayerMatch)
    {
        return il =>
        {
            ILCursor c = new(il);
            c.RemoveHealthCapCheck(LoadPlayerMatch);
        };
    }
}
