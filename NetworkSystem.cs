using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OverhealthMod;

public class NetworkSystem : ModSystem
{
    public override bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
    {
        // Sync overhealth along with life
        if (msgType == MessageID.PlayerLifeMana)
        {
            int playerIdx = number;
            OverhealthPlayer modPlayer = Main.player[playerIdx].GetModPlayer<OverhealthPlayer>();
            modPlayer.SyncPlayer(-1, number, false);
        }

        return false;
    }
}