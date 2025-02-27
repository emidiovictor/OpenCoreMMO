using NeoServer.Game.Contracts.Creatures;
using NeoServer.Server.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing
{
    public class PlayerModesPacket : OutgoingPacket
    {
        private readonly IPlayer player;
        public PlayerModesPacket(IPlayer player)
        {
            this.player = player;
        }
        public override void WriteToMessage(INetworkMessage message)
        {
            message.AddByte((byte)GameOutgoingPacketType.PlayerModes);
            message.AddByte((byte)player.FightMode);
            message.AddByte((byte)player.ChaseMode);
            message.AddByte(player.SecureMode);
        }
    }
}