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
    class Secondary : BaseShyvanaState
    {
        private float duration;
        private float baseDuration = 0.65f;
        private float minFireDelay;
        private bool hasFired;
        private float damageCoefficient = 3.15f;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / base.attackSpeedStat;
            minFireDelay = duration * 0.22f;

            bool isMoving = animator.GetBool("isMoving");
            bool isGrounded = animator.GetBool("isGrounded");
            if (isMoving || !isGrounded)
            {
                base.PlayAnimation("Gesture, Override", "Spell3", "Spell3", duration);
            }
            else
            {
                base.PlayAnimation("FullBody, Override", "Spell3", "Spell3", duration);
            }
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
            AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Fireball_Cast, base.gameObject);
            EffectManager.SimpleMuzzleFlash(Prefabs.Load<GameObject>("RoR2/Base/BFG/MuzzleflashBFG.prefab"), base.gameObject, "handMuzzleL", false);
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                ProjectileManager.instance.FireProjectile(Prefabs.fireProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction),
                           base.gameObject, base.characterBody.damage * damageCoefficient, 240, base.RollCrit(), DamageColorIndex.Default);
            }
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
