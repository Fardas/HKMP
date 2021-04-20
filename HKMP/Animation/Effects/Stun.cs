﻿using HKMP.Util;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace HKMP.Animation.Effects {
    public class Stun : AnimationEffect {
        public override void Play(GameObject playerObject, bool[] effectInfo) {
            // Remove all effects/attacks/spells related animations
            MonoBehaviourUtil.DestroyAllChildren(playerObject.FindGameObjectInChildren("Attacks"));
            MonoBehaviourUtil.DestroyAllChildren(playerObject.FindGameObjectInChildren("Effects"));
            MonoBehaviourUtil.DestroyAllChildren(playerObject.FindGameObjectInChildren("Spells"));
            
            // Get the player effects object to put new effects in
            var playerEffects = playerObject.FindGameObjectInChildren("Effects");

            // If either the charge audio of the lines animation objects exists,
            // the player was probably focussing, so we start the Focus End effect
            if (playerObject.FindGameObjectInChildren("Charge Audio") != null ||
                playerObject.FindGameObjectInChildren("Lines Anim") != null) {
                AnimationManager.FocusEnd.Play(playerObject);
            }

            // Find the shell animation if it exists
            var shellAnimation = playerEffects.FindGameObjectInChildren("Shell Animation");
            var lastShellHit = false;

            // It might be suffixed with "Last" if it was the last baldur hit the player could take
            if (shellAnimation == null) {
                shellAnimation = playerEffects.FindGameObjectInChildren("Shell Animation Last");
                lastShellHit = true;
            }
            
            // If either version was found, we need to play some animations and sounds
            if (shellAnimation != null) {
                // Get the sprite animator and play the correct sounds if the shell broke or not
                var shellAnimator = shellAnimation.GetComponent<tk2dSpriteAnimator>();
                if (lastShellHit) {
                    shellAnimator.Play("Break");
                } else {
                    shellAnimator.Play("Impact");
                }

                // Destroy the animation after some time either way
                Object.Destroy(shellAnimation, 1.5f);
                
                // Get a new audio object and source and play the blocker impact clip
                var audioObject = AudioUtil.GetAudioSourceObject(playerEffects);
                var audioSource = audioObject.GetComponent<AudioSource>();
                audioSource.clip = HeroController.instance.blockerImpact;
                audioSource.Play();

                // Also destroy this object after some time
                Object.Destroy(audioObject, 2.0f);

                // If it was the last hit, we spawn some debris (bits) that fly of the shell as it breaks
                if (lastShellHit) {
                    var charmEffects = HeroController.instance.gameObject.FindGameObjectInChildren("Charm Effects");
                    var blockerShieldObject = charmEffects.FindGameObjectInChildren("Blocker Shield");
                    var shellFsm = blockerShieldObject.LocateMyFSM("Control");

                    // Since this is replicated 5 times in the FSM, we loop 5 times
                    for (var i = 1; i < 6; i++) {
                        var flingObjectAction = shellFsm.GetAction<FlingObjectsFromGlobalPool>("Bits", i);

                        // These values are from the FSM
                        var config = new FlingUtils.Config {
                            Prefab = flingObjectAction.gameObject.Value,
                            AmountMin = 2,
                            AmountMax = 2,
                            AngleMin = 40,
                            AngleMax = 140,
                            SpeedMin = 15,
                            SpeedMax = 22
                        };

                        // Spawn, fling and store the bits
                        var spawnedBits = FlingUtils.SpawnAndFling(
                            config,
                            playerEffects.transform,
                            Vector3.zero
                        );
                        // Destroy all the bits after some time
                        foreach (var bit in spawnedBits) {
                            Object.Destroy(bit, 2.0f);
                        }
                    }
                }
            }
            
            // TODO: maybe add an option for playing the hit sound as it is very uncanny
            // Being used to only hearing this when you get hit
            
            // Obtain the hit audio clip
            var heroAudioController = HeroController.instance.gameObject.GetComponent<HeroAudioController>();
            var takeHitClip = heroAudioController.takeHit.clip;

            // Get a new audio source and play the clip
            var takeHitAudioObject = AudioUtil.GetAudioSourceObject(playerObject);
            var takeHitAudioSource = takeHitAudioObject.GetComponent<AudioSource>();
            takeHitAudioSource.clip = takeHitClip;
            // Decrease volume, since otherwise it is quite loud in contrast to the local player hit sound
            takeHitAudioSource.volume = 0.5f;
            takeHitAudioSource.Play();

            Object.Destroy(takeHitAudioObject, 3.0f);
        }

        public override bool[] GetEffectInfo() {
            return null;
        }
    }
}