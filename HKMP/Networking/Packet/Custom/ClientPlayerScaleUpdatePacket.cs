﻿using UnityEngine;

namespace HKMP.Networking.Packet.Custom {
    public class ClientPlayerScaleUpdatePacket : Packet, IPacket {
        
        public int Id { get; set; }
        public Vector3 Scale { get; set; }

        public ClientPlayerScaleUpdatePacket() {
        }
        
        public ClientPlayerScaleUpdatePacket(Packet packet) : base(packet) {
        }
        
        public void CreatePacket() {
            Reset();
            
            Write(PacketId.ClientPlayerScaleUpdate);

            Write(Id);
            
            Write(Scale);

            WriteLength();
        }

        public void ReadPacket() {
            Id = ReadInt();
            Scale = ReadVector3();
        }
    }
}