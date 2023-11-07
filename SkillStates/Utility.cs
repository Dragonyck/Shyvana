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
    class Utility : BaseSkillState
    {
        private float duration = 8;
        private float stopwatch;
        private float damageCoefficient = 7.5f;
        private uint ID;
        private GameObject fireEffect;

        public override void OnEnter()
        {
            base.OnEnter();
            fireEffect = UnityEngine.Object.Instantiate(Prefabs.immolateEffect, base.transform);
            fireEffect.transform.localPosition = Vector3.zero;
            if (NetworkServer.active)
            {
                base.characterBody.AddTimedBuff(Prefabs.flameBuff, duration);
            }
            Fire();
            AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Immolate_Cast, base.gameObject);
            ID = AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Immolate_Activate, base.gameObject);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= 0.5f)
            {
                stopwatch = 0;
                Fire();
            }
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        private void Fire()
        {
            if (base.isAuthority)
            {
                new BlastAttack
                {
                    attacker = base.gameObject,
                    baseDamage = this.damageStat * damageCoefficient,
                    baseForce = 0f,
                    crit = base.RollCrit(),
                    damageType = DamageType.BypassBlock,
                    falloffModel = BlastAttack.FalloffModel.None,
                    procCoefficient = 1,
                    radius = 4.5f,
                    position = base.characterBody.corePosition,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    impactEffect = EffectCatalog.FindEffectIndexFromPrefab(Prefabs.fireImmolateHitEffect),
                    teamIndex = base.teamComponent.teamIndex,
                    damageColorIndex = DamageColorIndex.Default
                }.Fire();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (fireEffect)
            {
                EntityState.Destroy(fireEffect);
            }
            AkSoundEngine.StopPlayingID(ID);
            AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Immolate_Deactivate, base.gameObject);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
