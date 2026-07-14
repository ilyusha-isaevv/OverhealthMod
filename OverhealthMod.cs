using System;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace OverhealthMod;

public partial class OverhealthMod : Mod
{
    public override void Load()
    {
        IL_MessageBuffer.GetData += ILModify_MessageBuffer_GetData;
    }

    public void ILModify_MessageBuffer_GetData(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            c.GotoNext(MoveType.After, i => i.MatchStfld<Player>(nameof(Player.statLife))); // case 16 (life sync)
            c.GotoNext(MoveType.After, i => i.MatchStfld<Player>(nameof(Player.statLife))); // case 66 (spirit heal)
            c.RemoveRange(17); // Remove health cap

            c.EmitLdloc(324); // Load Player
            c.EmitCall(typeof(OverhealthPlayer).GetMethod(nameof(OverhealthPlayer.CapOverhealth), [typeof(Player)]));
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(this, il);
        }
    }
}
