using System.IO;
using Terraria;
using Terraria.ID;

namespace OverhealthMod;



public partial class OverhealthMod
{
    internal enum MessageType : byte
    {
        SyncOverhealth
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        MessageType messageType = (MessageType)reader.ReadByte();

        switch (messageType)
        {
            case MessageType.SyncOverhealth:
                byte playerIndex = reader.ReadByte();
                OverhealthPlayer player = Main.player[playerIndex].GetModPlayer<OverhealthPlayer>();
                player.RecieveSyncPlayer(reader);

                if (Main.netMode == NetmodeID.Server)
                    player.SyncPlayer(-1, playerIndex, false);
                break;
        }
    }
}