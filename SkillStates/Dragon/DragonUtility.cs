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
    class DragonUtility : EntityStates.Croco.Leap
    {
        private bool hasSkipped = false;
        public override void DoImpactAuthority()
        {   
            base.DetonateAuthority();
            Vector3 footPosition = base.characterBody.footPosition;
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = Prefabs.fireBallProjectileDotZone,
                crit = this.isCritAuthority,
                force = 0f,
                damage = this.damageStat,
                owner = base.gameObject,
                rotation = Quaternion.identity,
                position = footPosition
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }
        public override void OnEnter()
        {
            fistEffectPrefab = new GameObject();
            blastEffectPrefab = Prefabs.dragonSlamEffect;
            base.OnEnter();
            if (blastDamageCoefficient == 0)
            {
                blastDamageCoefficient = 3.2f;
            }
            base.characterMotor.velocity *= 2;
            base.modelAnimator.SetBool("skip", false);
            base.PlayAnimation("FullBody, Override", "Leap");
            AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Wing_Flap_Hard, base.gameObject);
            EffectManager.SimpleMuzzleFlash(Prefabs.leapEffect, base.gameObject, "base", false);
        }
        public override void FixedUpdate()
        {
            if (base.fixedAge >= 0.4f && base.estimatedVelocity.y <= 25)
            {
                base.modelAnimator.SetBool("skip", true);
            }
            if (base.fixedAge >= 0.4f && base.skillLocator.utility.stock > 0 && base.isAuthority && base.inputBank.skill3.down && !hasSkipped)
            {
                hasSkipped = true;
                base.skillLocator.utility.DeductStock(1);
                DragonUtility state = new DragonUtility();
                state.blastDamageCoefficient = blastDamageCoefficient + 3.2f;
                this.outer.SetNextState(state);
                return;
            }
            if (base.isAuthority && base.inputBank.skill2.down)
            {
                this.outer.SetNextStateToMain();
                return;
            }
            base.FixedUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
            base.modelAnimator.SetTrigger("trigger");
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
