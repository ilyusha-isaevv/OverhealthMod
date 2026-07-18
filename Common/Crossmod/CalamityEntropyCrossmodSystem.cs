using System;
using CalamityEntropy.Common;
using CalamityEntropy.Content.Items.Weapons;
using CalamityEntropy.Content.Projectiles;
using MonoMod.Cil;
using OverhealthMod.Utils;
using Terraria.ModLoader;

namespace OverhealthMod.Common.Crossmod;

[ExtendsFromMod("CalamityEntropy")]
public class CalamityEntropyCrossmodSystem : ModSystem
{
    public override void Load()
    {
        // NPC On-Hit Lifesteal Cap Check
        QuickIL.EditMethod<EGlobalNPC>(nameof(EGlobalNPC.onHurt), CommonIL.RemoveHealthCapCheck_Bge);
        QuickIL.EditMethod<SacrificalDagger>(nameof(SacrificalDagger.OnHitNPC), IlModify_SacrificalDagger_OnHitNPC);

        // Do not patch this, because healing is too small
        // QuickIL.EditMethod<EModPlayer>(nameof(EModPlayer.PostUpdate), ...);
        // QuickIL.EditMethod<AzafureHealingTowerSentry>(nameof(AzafureHealingTowerSentry.AI), ...);
    }

    private void IlModify_SacrificalDagger_OnHitNPC(ILContext il)
    {
        ILCursor c = new(il);
        c.RemoveRange(12);
    }
}

