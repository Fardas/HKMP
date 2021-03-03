﻿using System;
using System.Collections.Generic;
using HKMP.Networking.Packet.Custom;
using HKMP.Util;

namespace HKMP.Networking.Packet {
    public delegate void ClientPacketHandler(IPacket packet);
    public delegate void GenericClientPacketHandler<in T>(T packet) where T : IPacket;
    public delegate void ServerPacketHandler(int id, IPacket packet);
    public delegate void GenericServerPacketHandler<in T>(int id, T packet) where T : IPacket;
    
    /**
     * Manages incoming packets by executing a corresponding registered handler
     */
    public class PacketManager {

        // Handlers that deal with data from the server intended for the client
        private readonly Dictionary<PacketId, ClientPacketHandler> _clientPacketHandlers;
        // Handlers that deal with data from the client intended for the server
        private readonly Dictionary<PacketId, ServerPacketHandler> _serverPacketHandlers;

        /**
         * Manages packets that are received by the given NetClient
         */
        public PacketManager() {
            _clientPacketHandlers = new Dictionary<PacketId, ClientPacketHandler>();
            _serverPacketHandlers = new Dictionary<PacketId, ServerPacketHandler>();
        }

        /**
         * Handle data received by a client
         */
        public void HandleClientData(byte[] data) {
            // Transform raw data into packets
            var packets = ByteArrayToPackets(data);
            // Execute corresponding packet handlers
            foreach (var packet in packets) {
                ExecuteClientPacketHandler(packet);
            }
        }

        /**
         * Handle data received by the server
         */
        public void HandleServerData(int id, byte[] data) {
            // Transform raw data into packets
            var packets = ByteArrayToPackets(data);
            
            // Execute corresponding packet handlers
            foreach (var packet in packets) {
                ExecuteServerPacketHandler(id, packet);
            }
        }

        /**
         * Executes the correct packet handler corresponding to this packet.
         * Assumes that the packet is not read yet.
         */
        private void ExecuteClientPacketHandler(Packet packet) {
            var packetId = packet.ReadPacketId();

            if (!_clientPacketHandlers.ContainsKey(packetId)) {
                Logger.Warn(this, $"There is no client packet handler registered for ID: {packetId}");
                return;
            }

            var instantiatedPacket = InstantiatePacket(packetId, packet);
            
            if (instantiatedPacket == null) {
                Logger.Warn(this, $"Could not instantiate client packet with ID: {packetId}");
                return;
            }
            
            // Read the packet data into the packet object before sending it to the packet handler
            instantiatedPacket.ReadPacket();

            // Invoke the packet handler for this ID on the Unity main thread
            ThreadUtil.RunActionOnMainThread(() => {
                _clientPacketHandlers[packetId].Invoke(instantiatedPacket);
            });
        }
        
        /**
         * Executes the correct packet handler corresponding to this packet.
         * Assumes that the packet is not read yet.
         */
        private void ExecuteServerPacketHandler(int id, Packet packet) {
            var packetId = packet.ReadPacketId();

            if (!_serverPacketHandlers.ContainsKey(packetId)) {
                Logger.Warn(this, $"There is no server packet handler registered for ID: {packetId}");
                return;
            }
            
            var instantiatedPacket = InstantiatePacket(packetId, packet);
            
            if (instantiatedPacket == null) {
                Logger.Warn(this, $"Could not instantiate server packet with ID: {packetId}");
                return;
            }

            // Read the packet data into the packet object before sending it to the packet handler
            instantiatedPacket.ReadPacket();
            
            // Invoke the packet handler for this ID on the Unity main thread
            ThreadUtil.RunActionOnMainThread(() => {
                _serverPacketHandlers[packetId].Invoke(id, instantiatedPacket);
            });
        }

        public void RegisterClientPacketHandler<T>(PacketId packetId, GenericClientPacketHandler<T> packetHandler) where T : IPacket {
            if (_clientPacketHandlers.ContainsKey(packetId)) {
                Logger.Error(this, $"Tried to register already existing client packet handler: {packetId}");
                return;
            }

            // We can't store these kinds of generic delegates in a dictionary,
            // so we wrap it in a function that casts it
            _clientPacketHandlers[packetId] = iPacket => {
                packetHandler((T) iPacket);
            };
        }

        public void DeregisterClientPacketHandler(PacketId packetId) {
            if (!_clientPacketHandlers.ContainsKey(packetId)) {
                Logger.Error(this, $"Tried to remove non-existent client packet handler: {packetId}");
                return;
            }

            _clientPacketHandlers.Remove(packetId);
        }
        
        public void RegisterServerPacketHandler<T>(PacketId packetId, GenericServerPacketHandler<T> packetHandler) where T : IPacket {
            if (_serverPacketHandlers.ContainsKey(packetId)) {
                Logger.Error(this, $"Tried to register already existing server packet handler: {packetId}");
                return;
            }

            // We can't store these kinds of generic delegates in a dictionary,
            // so we wrap it in a function that casts it
            _serverPacketHandlers[packetId] = (id, iPacket) => {
                packetHandler(id, (T) iPacket);
            };
        }

        public void DeregisterServerPacketHandler(PacketId packetId) {
            if (!_serverPacketHandlers.ContainsKey(packetId)) {
                Logger.Error(this, $"Tried to remove non-existent server packet handler: {packetId}");
                return;
            }

            _serverPacketHandlers.Remove(packetId);
        }

        private List<Packet> ByteArrayToPackets(byte[] data) {
            var packets = new List<Packet>();

            // Keep track of current index in the data array
            var readIndex = 0;
            
            // The only break from this loop is when there is no new packet to be read
            do {
                // If there is still an int (4 bytes) to read in the data,
                // it represents the next packet's length
                var packetLength = 0;
                if (data.Length - readIndex >= 4) {
                    packetLength = BitConverter.ToInt32(data, readIndex);
                    readIndex += 4;
                }
                
                // There is no new packet, so we can break
                if (packetLength <= 0) {
                    break;
                }

                // Read the next packet's length in bytes
                var packetData = new byte[packetLength];
                for (var i = 0; i < packetLength; i++) {
                    packetData[i] = data[readIndex + i];
                }

                readIndex += packetLength;
                
                // Create a packet out of this byte array
                var newPacket = new Packet(packetData);
                
                // Add it to the list of parsed packets
                packets.Add(newPacket);
            } while (true);
            
            return packets;
        }

        /**
         * We somehow need to instantiate the correct implementation of the
         * IPacket, so we do it here
         */
        private IPacket InstantiatePacket(PacketId packetId, Packet packet) {
            switch (packetId) {
                case PacketId.HelloServer:
                    return new HelloServerPacket(packet);
                case PacketId.PlayerDisconnect:
                    return new PlayerDisconnectPacket(packet);
                case PacketId.ServerShutdown:
                    return new ServerShutdownPacket(packet);
                case PacketId.PlayerChangeScene:
                    return new PlayerChangeScenePacket(packet);
                case PacketId.PlayerEnterScene:
                    return new PlayerEnterScenePacket(packet);
                case PacketId.PlayerLeaveScene:
                    return new PlayerLeaveScenePacket(packet);
                case PacketId.ServerPlayerPositionUpdate:
                    return new ServerPlayerPositionUpdatePacket(packet);
                case PacketId.ClientPlayerPositionUpdate:
                    return new ClientPlayerPositionUpdatePacket(packet);
                case PacketId.ServerPlayerScaleUpdate:
                    return new ServerPlayerScaleUpdatePacket(packet);
                case PacketId.ClientPlayerScaleUpdate:
                    return new ClientPlayerScaleUpdatePacket(packet);
                case PacketId.ServerPlayerAnimationUpdate:
                    return new ServerPlayerAnimationUpdatePacket(packet);
                case PacketId.ClientPlayerAnimationUpdate:
                    return new ClientPlayerAnimationUpdatePacket(packet);
                case PacketId.ServerPlayerDeath:
                    return new ServerPlayerDeathPacket(packet);
                case PacketId.ClientPlayerDeath:
                    return new ClientPlayerDeathPacket(packet);
                case PacketId.GameSettingsUpdated:
                    return new GameSettingsUpdatePacket(packet);
                default:
                    return null;
            }
        }
        
    }
}