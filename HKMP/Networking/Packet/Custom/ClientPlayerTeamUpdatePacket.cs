using HKMP.Game;

namespace HKMP.Networking.Packet.Custom {
    public class ClientPlayerTeamUpdatePacket : Packet, IPacket {

        public ushort Id { get; set; }
        
        public Team Team { get; set; }
        
        public ClientPlayerTeamUpdatePacket() {
        }

        public ClientPlayerTeamUpdatePacket(Packet packet) : base(packet) {
        }
        
        public Packet CreatePacket() {
            Reset();

            Write(PacketId.PlayerTeamUpdate);

            Write(Id);
            Write((byte) Team);
            
            WriteLength();

            return this;
        }

        public void ReadPacket() {
            Id = ReadUShort();
            Team = (Team) ReadByte();
        }
    }
}