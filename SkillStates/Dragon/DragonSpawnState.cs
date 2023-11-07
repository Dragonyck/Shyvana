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
    class DragonSpawnState : BaseShyvanaState
    {
        private Vector3 forward;
        private CameraTargetParams.AimRequest request;
        private int index;
        private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        private float distance;
        private Vector3 endPos;
        public bool e;

        public override void OnEnter()
        {
            base.OnEnter();
            if (base.isAuthority && behaviour.behaviour.positions.Length == 0)
            {
                this.outer.SetNextStateToMain();
                return;
            }
            base.animator.SetBool("isGrounded", false);
            base.PlayAnimation("Body", "Intro");
            base.SmallHop(base.characterMotor, 10);
            if (base.cameraTargetParams)
            {
                request = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }
            AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Transform_Roar, base.gameObject);
            if (base.isAuthority)
            {
                endPos = behaviour.behaviour.positions[behaviour.behaviour.positions.Length - 1];
                distance = Vector3.Distance(endPos, base.transform.position);
                forward = (endPos - base.transform.position).normalized;
            }
            base.characterMotor.Motor.ForceUnground();

            EffectManager.SimpleMuzzleFlash(Prefabs.dragonTransformEffect, base.gameObject, "swingMuzzle", false);
        }
        public override void Update()
        {
            base.Update();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                base.characterMotor.velocity = Vector3.zero;
                base.characterDirection.forward = forward;
                if (!e)
                {
                    var a = new DragonSpawnState();
                    a.e = true;
                    this.outer.SetNextState(a);
                    return;
                }
                if (behaviour.behaviour.positions.Length == 0 || index == behaviour.behaviour.positions.Length)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
                if (base.fixedAge >= 0.4f && base.isGrounded || Vector3.Distance(endPos, base.transform.position) <= 4 || base.fixedAge >= 2)
                {
                    EffectData efectData = new EffectData()
                    {
                        origin = base.characterBody.footPosition,
                        scale = 12
                    };
                    EffectManager.SpawnEffect(Prefabs.dragonSlamEffect, efectData, true);

                    new BlastAttack
                    {
                        attacker = base.gameObject,
                        baseDamage = this.damageStat * 3.8f,
                        baseForce = 0f,
                        crit = base.RollCrit(),
                        damageType = DamageType.Stun1s,
                        falloffModel = BlastAttack.FalloffModel.None,
                        procCoefficient = 1,
                        radius = 12,
                        position = base.characterBody.corePosition,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        impactEffect = EffectCatalog.FindEffectIndexFromPrefab(Prefabs.fireProjectileExplosion),
                        teamIndex = base.teamComponent.teamIndex,
                        damageColorIndex = DamageColorIndex.Default
                    }.Fire();

                    this.outer.SetNextStateToMain();
                    return;
                }
                float speed = curve.Evaluate(base.fixedAge / Util.Remap(distance, 0f, 280f, 0.5f, 1.5f));
                base.characterMotor.Motor.SetPosition(Vector3.Lerp(base.transform.position, behaviour.behaviour.positions[index], 195 * speed));
                if (base.transform.position == behaviour.behaviour.positions[index] && index < behaviour.behaviour.positions.Length - 1)
                {
                    index += 1;
                }
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (request != null && base.cameraTargetParams)
            {
                request.Dispose();
            }
            if (behaviour.behaviour)
            {
                behaviour.behaviour.transformed = true;
            }
            base.animator.SetBool("isGrounded", true);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
