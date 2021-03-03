﻿using HKMP.Networking.Packet.Custom;
using ModCommon;
using ModCommon.Util;
using UnityEngine;

// TODO: perhaps play the screen shake also when our local player is close enough
namespace HKMP.Animation.Effects {
    public class CrystalDashHitWall : IAnimationEffect {
        public void Play(GameObject playerObject, ClientPlayerAnimationUpdatePacket packet) {
            // Get both the local player and remote player effects object
            var heroEffects = HeroController.instance.gameObject.FindGameObjectInChildren("Effects");
            var playerEffects = playerObject.FindGameObjectInChildren("Effects");
            
            // Play the end animation for the crystal dash trail
            playerEffects.FindGameObjectInChildren("SD Trail").GetComponent<tk2dSpriteAnimator>().Play("SD Trail End");
            
            // Instantiate the wall hit effect and make sure to destroy it once the FSM is done
            var wallHitEffect = Object.Instantiate(heroEffects.FindGameObjectInChildren("Wall Hit Effect"), playerEffects.transform);
            wallHitEffect.LocateMyFSM("FSM").InsertMethod("Destroy", 1, () => Object.Destroy(wallHitEffect));
        }

        public void PreparePacket(ServerPlayerAnimationUpdatePacket packet) {
        }
    }
}