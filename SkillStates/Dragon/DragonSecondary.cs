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
using EntityStates.Mage;
using EntityStates.LemurianBruiserMonster;

namespace Shyvana
{
    class DragonSecondary : BaseShyvanaState
    {
		private ChildLocator childLocator;
		private Transform muzzleTransform;
		private float stopwatch;
		private float burstStopwatch;
		private float attackDelay;
		private Ray aimRay;
		private GameObject hitEffectPrefab = Prefabs.Load<GameObject>("RoR2/Base/Engi/ImpactEngiTurret.prefab");
		private float startup = 0.2f;
		public override void OnEnter()
		{
			base.OnEnter();
			base.StartAimMode(2);
			attackDelay = 0.125f;// / base.attackSpeedStat;
			childLocator = base.GetModelChildLocator();
			muzzleTransform = childLocator.FindChild("jawMuzzle");
			AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Wing_Flap_Hard, base.gameObject);
			aimRay = base.GetAimRay();
			base.PlayAnimation("FullBody, Override", "Spell2");
			base.LockSkills();
		}
		public override void Update()
		{
			base.Update();
			UpdateFlamethrowerEffect();
			if (animator)
            {
				animator.SetBool("isMoving", false);
			}
		}
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= startup && !behaviour.effect)
			{
				AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Dragon_Fireball_Cast, base.gameObject);
				behaviour.ID = AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Dragon_Flames, base.gameObject);
				behaviour.effect = UnityEngine.Object.Instantiate<GameObject>(Prefabs.flameBreathEffect, muzzleTransform);
				behaviour.effect.transform.localPosition = Vector3.zero;
				behaviour.effect.transform.localScale = Vector3.one * -1;
			}
			base.characterBody.isSprinting = false;
			if (base.isAuthority)
			{
				float num = base.characterMotor.velocity.y;
				num = Mathf.MoveTowards(num, JetpackOn.hoverVelocity, JetpackOn.hoverAcceleration / 1.75f * Time.fixedDeltaTime);
				base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, num, base.characterMotor.velocity.z);
			}
			if (base.characterDirection)
			{
				Vector2 vector = Util.Vector3XZToVector2XY(base.GetAimRay().direction);
				if (vector != Vector2.zero)
				{
					vector.Normalize();
					var direction = new Vector3(vector.x, 0f, vector.y).normalized;
					base.characterDirection.moveVector = direction;
				}
			}
			if (base.fixedAge >= startup)
			{
				stopwatch += Time.fixedDeltaTime;
				if (stopwatch >= attackDelay)
				{
					stopwatch = 0;
					Fire();
				}
			}
			if (base.isAuthority && !base.inputBank.skill2.down || behaviour.powerupValue <= 0)
			{
				this.outer.SetNextState(new DragonSecondaryEnd());
			}
			UpdateFlamethrowerEffect();
		}
		private void UpdateFlamethrowerEffect()
		{
			if (behaviour.effect)
			{
				behaviour.effect.transform.forward = base.GetAimRay().direction;
			}
		}
		private void Fire()
		{
			if (base.isAuthority)
			{
				Ray aimRay = base.GetAimRay();
				new BulletAttack
				{
					owner = base.gameObject,
					weapon = base.gameObject,
					origin = aimRay.origin,
					aimVector = aimRay.direction,
					minSpread = 0f,
					damage = this.damageStat * 0.5f * base.attackSpeedStat,
					force = 80,
					muzzleName = "jawMuzzle",
					hitEffectPrefab = hitEffectPrefab,
					isCrit = base.RollCrit(),
					radius = 2,
					falloffModel = BulletAttack.FalloffModel.None,
					stopperMask = LayerIndex.world.mask,
					procCoefficient = Flamebreath.procCoefficientPerTick,
					maxDistance = Flamebreath.maxDistance,
					smartCollision = true,
					damageType = DamageType.BypassOneShotProtection
				}.Fire();
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
	class DragonSecondaryEnd : BaseShyvanaState
    {
        public override void OnEnter()
        {
            base.OnEnter();
			base.UnlockSkills();
			animator.SetTrigger("trigger");
			if (behaviour.effect)
			{
				EntityState.Destroy(behaviour.effect);
			}
			AkSoundEngine.StopPlayingID(behaviour.ID);
			if (base.isAuthority)
            {
				this.outer.SetNextStateToMain();
			}
		}
        public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}
}
