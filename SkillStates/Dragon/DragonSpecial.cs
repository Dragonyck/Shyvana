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
    class DragonSpecial : BaseShyvanaState
    {
        private float duration;
        private float baseDuration = 0.65f;
        private float minFireDelay;
        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / base.attackSpeedStat;
            minFireDelay = duration * 0.15f;
            base.AdjustModelForwardAim();
            base.PlayAnimation("FullBody, Override", "Spell1", "Spell1", duration);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= minFireDelay && !hasFired)
            {
                hasFired = true;
                Fire();
            }
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        private void Fire()
        {
            AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Dragon_Fireball_Cast, base.gameObject);
            EffectManager.SimpleMuzzleFlash(Prefabs.Load<GameObject>("RoR2/Base/BFG/MuzzleflashBFG.prefab"), base.gameObject, "handMuzzleL", false);
            Ray aimRay = base.GetAimRay();
            if (base.isAuthority)
            {
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = this.characterBody.damage * 7f,
                    damageTypeOverride = DamageType.PercentIgniteOnHit,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 500,
                    owner = base.gameObject,
                    position = aimRay.origin,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = Prefabs.fireBallProjectile,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    useFuseOverride = false,
                    useSpeedOverride = true,
                    speedOverride = 180,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
            base.characterMotor.velocity += -aimRay.direction * 40;
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
