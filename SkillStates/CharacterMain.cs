using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine.UI;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;

namespace Shyvana
{
    class ShyvanaSpawnState : SpawnTeleporterState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (base.characterBody.masterObject)
            {
                var behaviour = base.characterBody.masterObject.GetComponent<MasterBehaviour>();
                if (behaviour && behaviour.despawned)
                {
                    behaviour.despawned = false;
                    hasTeleported = true;
                    duration = 0;
                    EffectManager.SimpleEffect(Prefabs.despawnEffect, base.characterBody.corePosition, Quaternion.identity, false);
                    AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Immolate_Deactivate, base.gameObject);
                }
            }
            this.characterModel.invisibilityCount--;
        }
        public override void OnExit()
        {
            base.OnExit();
            if (NetworkServer.active)
            {
                base.characterBody.ClearTimedBuffs(RoR2Content.Buffs.HiddenInvincibility);
            }
        }
    }

    class CharacterMain : GenericCharacterMain
    {
        private uint ID;
        private bool playingID;
        private ShyvanaBehaviour behaviour;
        public override void OnEnter()
        {
            base.OnEnter();
            behaviour = base.GetComponent<ShyvanaBehaviour>();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.characterBody.outOfCombatStopwatch >= 2 && base.characterBody.outOfDangerStopwatch >= 2 && behaviour.canExecute)
            {
                if (!playingID)
                {
                    playingID = true;
                    ID = AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Dragon_Idle, base.gameObject);
                }
            }
            else if (playingID)
            {
                playingID = false;
                AkSoundEngine.StopPlayingID(ID);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            AkSoundEngine.StopPlayingID(ID);
        }
    }
}
