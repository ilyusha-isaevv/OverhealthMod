using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;

namespace OverhealthMod.Utils;

/// <summary>
/// Contains common IL edits.
/// </summary>
public static class CommonIL
{
    #region Utilities
    /// <summary>
    /// Same as general Match<T>, but works with integer indices for <see cref="OpCodes.Ldloc"/> and <see cref="OpCodes.Ldloc_S"/>.
    /// </summary>
    internal static bool CustomMatch<T>(this Instruction i, OpCode opcode, T value)
    {
        if ((opcode == OpCodes.Ldloc || opcode == OpCodes.Ldloc_S) && value is int intValue)
            return i.MatchLdloc(intValue);
        return i.Match(opcode, value);
    }

    /// <summary>
    /// Checks if two instructions are equivalent (same OpCode and Operand).
    /// </summary>
    internal static bool IsEquivalent(this Instruction a, Instruction b) => a.OpCode == b.OpCode && a.Operand == b.Operand;
    #endregion

    #region Cursor Extensions
    /// <summary>
    /// Moves before
    /// <code>
    /// if (... {player}.statLife &lt;op&gt; {player}.statLifeMax2 ...)
    /// </code>
    /// </summary>
    internal static void GotoHealthCapCheck(this ILCursor c, OpCode branchOpcode, OpCode shortBranchOpcode)
    {
        while (true)
        {
            c.GotoNext(MoveType.Before, i => i.Next.MatchLdfld<Player>(nameof(Player.statLife)));

            if (!c.Next.IsEquivalent(c.Instrs[c.Index + 2])) // Load player 
                continue;
            if (!c.Instrs[c.Index + 3].MatchLdfld<Player>(nameof(Player.statLifeMax2)))
                continue;
            if (!c.Instrs[c.Index + 4].Match(branchOpcode) && !c.Instrs[c.Index + 4].Match(shortBranchOpcode))
                continue;
            break;
        }
    }

    /// <summary>
    /// Moves before
    /// <code>
    /// if (... {player}.statLife &lt; {player}.statLifeMax2 ...)
    /// </code>
    /// </summary>
    public static void GotoHealthCapCheck_Bge(this ILCursor c) => c.GotoHealthCapCheck(OpCodes.Bge, OpCodes.Bge_S);

    /// <summary>
    /// Moves before 
    /// <code>
    /// if (... {player}.statLife &gt; {player}.statLifeMax2 ...)
    /// </code>
    /// </summary>
    public static void GotoHealthCapCheck_Ble(this ILCursor c) => c.GotoHealthCapCheck(OpCodes.Ble, OpCodes.Ble_S);

    /// <summary>
    /// Moves before 
    /// <code>
    /// if (... {player}.statLife &gt;= {player}.statLifeMax2 ...)
    /// </code>
    /// </summary>
    public static void GotoHealthCapCheck_Blt(this ILCursor c) => c.GotoHealthCapCheck(OpCodes.Blt, OpCodes.Blt_S);

    /// <summary>
    /// Moves before 
    /// <code>
    /// if (... {player}.statLife &gt; {player}.statLifeMax2)
    ///     {player}.statLife = {player}.statLifeMax2;
    /// </code>
    /// </summary>
    internal static void GotoHealthCap(this ILCursor c)
    {
        while (true)
        {
            c.GotoHealthCapCheck_Ble();

            if (!c.Next.IsEquivalent(c.Instrs[c.Index + 5]) || !c.Next.IsEquivalent(c.Instrs[c.Index + 6])) // Load player instructions
                continue;
            if (!c.Instrs[c.Index + 7].MatchLdfld<Player>(nameof(Player.statLifeMax2)))
                continue;
            if (!c.Instrs[c.Index + 8].MatchStfld<Player>(nameof(Player.statLife)))
                continue;
            break;
        }
    }

    /// <summary>
    /// Moves before the <c>Main.player[index].statLife &lt;op&gt; Main.player[index].statLifeMax2</c>
    /// </summary>
    /// <param name="c"></param>
    public static void GotoHealthCapCheck_Array(this ILCursor c)
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
    #endregion

    #region Manipulators
    /// <summary>
    /// Removes
    /// <code>
    /// if (... {player}.statLife &lt; {player}.statLifeMax2 ...)
    /// </code>
    /// </summary>
    public static void RemoveHealthCapCheck_Bge(ILContext il)
    {
        ILCursor c = new(il);
        c.GotoHealthCapCheck_Bge();
        c.RemoveRange(5); // Remove health cap check
    }

    /// <summary>
    /// Removes
    /// <code>
    /// if (... {player}.statLife &gt;= {player}.statLifeMax2 ...)
    /// </code>
    /// </summary>
    public static void RemoveHealthCapCheck_Blt(ILContext il)
    {
        ILCursor c = new(il);
        c.GotoHealthCapCheck_Blt();
        c.RemoveRange(5); // Remove health cap check
    }

    /// <summary>
    /// Removes
    /// <code>
    /// if (... Main.player[index].statLife ?? Main.player[index].statLifeMax2 ...)
    /// </code>
    /// </summary>
    public static void RemoveHealthCapCheck_Array(ILContext il)
    {
        ILCursor c = new(il);
        c.GotoHealthCapCheck_Array();
        c.RemoveRange(9);
    }

    /// <summary>
    /// Replaces
    /// <code>
    /// if (... {player}.statLife &lt; {player}.statLifeMax2))
    ///     {player}.statLife = {player}.statLifeMax2;
    /// </code>
    /// with 
    /// <code>
    /// OverhealthPlayer.CapOverhealth({player});
    /// </code>
    /// </summary>
    /// <param name="il"></param>
    public static void ReplaceHealthCapWithCapOverhealth(ILContext il)
    {
        ILCursor c = new(il);
        c.GotoHealthCap();
        Instruction loadPlayerInstruction = c.Next;

        c.RemoveRange(9); // Remove health cap check and assigning max health to life
        c.Emit(loadPlayerInstruction.OpCode, loadPlayerInstruction.Operand);
        c.EmitCall(typeof(OverhealthPlayer).GetMethod(nameof(OverhealthPlayer.CapOverhealth), [typeof(Player)]));
    }
    #endregion
}
