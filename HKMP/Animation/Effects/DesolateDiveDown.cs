﻿using HKMP.Networking.Packet.Custom;
using UnityEngine;

namespace HKMP.Animation.Effects {
    /**
     * The animation effect during the movement of going down from a Desolate Dive.
     */
    public class DesolateDiveDown : QuakeDownBase {
        public override void Play(GameObject playerObject, ClientPlayerAnimationUpdatePacket packet) {
            // Call the play method with the correct Q Trail prefab name
            Play(playerObject, packet, "Q Trail");
        }
    }
}