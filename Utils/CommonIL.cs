using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace OverhealthMod.Utils;

/// <summary>
/// Contains common IL edits.
/// </summary>
public static class CommonIL
{
    /// <summary>
    /// Same as general Match<T>, but works with integer indices for <see cref="OpCodes.Ldloc"/> and <see cref="OpCodes.Ldloc_S"/>.
    /// </summary>
    private static bool CustomMatch<T>(this Instruction i, OpCode opcode, T value)
    {
        if ((opcode == OpCodes.Ldloc || opcode == OpCodes.Ldloc_S) && value is int intValue)
            return i.MatchLdloc(intValue);
        return i.Match(opcode, value);
    }

    private static void GotoHealthCap(this ILCursor c, OpCode loadPlayerOpcode)
    {
        c.GotoNext(MoveType.Before,
            i => i.Match(loadPlayerOpcode) && i.Next.MatchLdfld<Player>(nameof(Player.statLife)) &&
            i.Next.Next.Match(loadPlayerOpcode) && i.Next.Next.Next.MatchLdfld<Player>(nameof(Player.statLifeMax2))
        ); // ... player.statLife ?? player.statLifeMax2 ...
    }

    private static void GotoHealthCap<T>(this ILCursor c, OpCode LoadPlayerOpcode, T loadPlayerValue)
    {
        c.GotoNext(MoveType.Before,
            i => i.CustomMatch(LoadPlayerOpcode, loadPlayerValue) && i.Next.MatchLdfld<Player>(nameof(Player.statLife)) &&
            i.Next.Next.CustomMatch(LoadPlayerOpcode, loadPlayerValue) && i.Next.Next.Next.MatchLdfld<Player>(nameof(Player.statLifeMax2))
        ); // ... player.statLife ?? player.statLifeMax2 ...
    }

    private static void GotoHealthCap_Array(this ILCursor c)
    {
        while (true)
        {
            c.GotoNext(MoveType.Before, i => i.MatchLdsfld<Main>(nameof(Main.player)));

            if (!c.Instrs[c.Index + 3].MatchLdfld<Player>(nameof(Player.statLife)))
                c.Index += 4;
            else if (!c.Instrs[c.Index + 4].MatchLdsfld<Main>(nameof(Main.player)))
                c.Index += 5;
            else if (!c.Instrs[c.Index + 7].MatchLdfld<Player>(nameof(Player.statLifeMax2)))
                c.Index += 8;
            else
                break;
        }
    }

    public static void GotoAndRemoveHealthCapCheck(this ILCursor c, OpCode loadPlayerOpcode)
    {
        c.GotoHealthCap(loadPlayerOpcode);
        c.RemoveRange(5); // Remove health cap check
    }

    public static void GotoAndRemoveHealthCapCheck<T>(this ILCursor c, OpCode loadPlayerOpcode, T loadPlayerValue)
    {
        c.GotoHealthCap(loadPlayerOpcode, loadPlayerValue);
        c.RemoveRange(5); // Remove health cap check
    }

    public static void GotoAndRemoveHealthCapCheck_Array(this ILCursor c)
    {
        c.GotoHealthCap_Array();
        c.RemoveRange(9); // Remove health cap check
    }

    public static void GotoAndReplaceHealthCapWithOverhealthCap(this ILCursor c, OpCode loadPlayerOpcode)
    {
        c.GotoHealthCap(loadPlayerOpcode);
        c.RemoveRange(9); // Remove health cap check and assigning max health to life

        c.Emit(loadPlayerOpcode);
        c.EmitCall(typeof(OverhealthPlayer).GetMethod(nameof(OverhealthPlayer.CapOverhealth), [typeof(Player)]));
    }

    public static void GotoAndReplaceHealthCapWithOverhealthCap<T>(this ILCursor c, OpCode loadPlayerOpcode, T loadPlayerValue)
    {
        c.GotoHealthCap(loadPlayerOpcode, loadPlayerValue);
        c.RemoveRange(9); // Remove health cap check and assigning max health to life

        c.Emit(loadPlayerOpcode, loadPlayerValue);
        c.EmitCall(typeof(OverhealthPlayer).GetMethod(nameof(OverhealthPlayer.CapOverhealth), [typeof(Player)]));
    }

    public static ILContext.Manipulator RemoveHealthCapCheck(OpCode loadPlayerOpcode)
    {
        return il =>
        {
            ILCursor c = new(il);
            c.GotoAndRemoveHealthCapCheck(loadPlayerOpcode);
        };
    }

    public static ILContext.Manipulator RemoveHealthCapCheck<T>(OpCode loadPlayerOpcode, T loadPlayerValue)
    {
        return il =>
        {
            ILCursor c = new(il);
            c.GotoAndRemoveHealthCapCheck(loadPlayerOpcode, loadPlayerValue);
        };
    }

    public static void RemoveHealthCapCheck_Array(ILContext il)
    {
        ILCursor c = new(il);
        c.GotoAndRemoveHealthCapCheck_Array();
    }

    public static ILContext.Manipulator ReplaceHealthCapWithOverhealthCap(OpCode loadPlayerOpcode)
    {
        return il =>
        {
            ILCursor c = new(il);
            c.GotoAndReplaceHealthCapWithOverhealthCap(loadPlayerOpcode);
        };
    }

    public static ILContext.Manipulator ReplaceHealthCapWithOverhealthCap<T>(OpCode loadPlayerOpcode, T loadPlayerValue)
    {
        return il =>
        {
            ILCursor c = new(il);
            c.GotoAndReplaceHealthCapWithOverhealthCap(loadPlayerOpcode, loadPlayerValue);
        };
    }
}
