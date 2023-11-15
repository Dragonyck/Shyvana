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
using EntityStates.Merc;
using UnityEngine.AddressableAssets;

namespace Shyvana
{
	class Primary : BaseShyvanaState
	{
		public virtual float comboDamageCoefficient
		{
			get
			{
				return 2.85f;

			}
		}
		public virtual bool dragonForm
		{
			get
			{
				return false;

			}
		}
		internal float forceMagnitude = GroundLight.forceMagnitude;
		internal float hitPauseDuration = GroundLight.hitPauseDuration;
		private float stopwatch;
		private float attackDuration;
		private float earlyExitDuration;
		private OverlapAttack overlapAttack;
		private float hitPauseTimer;
		private bool isInHitPause;
		private bool hasSwung;
		private bool hasHit;
		public int comboState = 1;
		private BaseState.HitStopCachedState hitStopCachedState;
		public GameObject swingEffectPrefab = Prefabs.swingEffect;
		public GameObject hitEffectPrefab;
		private bool hop;
		private bool addedFury;
		private float furyGain = 0.03f;

		public override void OnEnter()
		{
			base.OnEnter();
			hop = true;
			base.StartAimMode(base.GetAimRay(), 3f, false);
			stopwatch = 0f;
			attackDuration = GroundLight.baseComboAttackDuration / base.attackSpeedStat;
			earlyExitDuration = GroundLight.baseEarlyExitDuration / base.attackSpeedStat;
			bool isMoving = animator.GetBool("isMoving");
			bool isGrounded = animator.GetBool("isGrounded");
			animator.SetFloat("Swing", 0);
			Transform modelTransform = base.GetModelTransform();

			string layerName = (isMoving || !isGrounded) ? "Gesture, Override" : "FullBody, Override";
			if (dragonForm)
			{
				layerName = "FullBody, Override";
				forceMagnitude *= 2;
			}
			base.PlayAnimation(layerName, "Attack" + comboState, "M1", attackDuration);
			overlapAttack = base.InitMeleeOverlap(comboDamageCoefficient, hitEffectPrefab, base.GetModelTransform(), "Swing");
			base.characterBody.SetAimTimer(attackDuration + 1f);
			hitEffectPrefab = dragonForm ? Prefabs.dragonSwingHitEffect : Prefabs.swingHitEffect;
			overlapAttack.hitEffectPrefab = hitEffectPrefab;
			AkSoundEngine.PostEvent(dragonForm ? Sounds.Play_Shyvana_Dragon_Auto_Cast : Sounds.Play_Shyvana_Auto_Cast, base.gameObject);
		}
		public override void OnExit()
		{
			base.OnExit();
		}
        public override void Update()
        {
            base.Update();
			if (dragonForm)
			{
				if (!hasHit)
                {
					base.characterMotor.velocity = base.characterMotor.velocity * 0.5f;
				}
				base.characterBody.isSprinting = false;
				animator.SetBool("isMoving", false);
			}
		}
        public override void FixedUpdate()
		{
			base.FixedUpdate();
			hitPauseTimer -= Time.fixedDeltaTime;
			if (base.isAuthority)
			{
				bool flag = base.FireMeleeOverlap(overlapAttack, animator, "Swing.active", forceMagnitude, true);
				hasHit = (hasHit || flag);
				if (flag)
				{
					if (hop && !base.characterMotor.isGrounded)
					{
						base.SmallHop(base.characterMotor, 5);
						hop = false;
					}
					if (!isInHitPause)
					{
						hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, animator, "M1");
						hitPauseTimer = hitPauseDuration / base.attackSpeedStat;
						isInHitPause = true;
					}
				}
                if (!hasHit && animator.GetFloat("Swing.active") > 0.1f && dragonForm)
				{
					Vector3 dir = base.GetAimRay().direction;
					var direction = new Vector3(dir.x, 0f, dir.z);
					base.characterMotor.rootMotion += direction * 25 * Time.fixedDeltaTime;
				}
				if (hitPauseTimer <= 0f && isInHitPause)
				{
					base.ConsumeHitStopCachedState(hitStopCachedState, base.characterMotor, animator);
					isInHitPause = false;
				}
			}
			if (animator.GetFloat("Swing.active") > 0f && !hasSwung)
			{
				hasSwung = true;
				if (dragonForm)
				{
					var muzzle = base.FindModelChild("swingMuzzle");
					muzzle.localScale = (comboState == 2) ? new Vector3(-1, 1, 1) : Vector3.one;
					EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, "swingMuzzle", false);
				}
			}
			if (hasHit && behaviour && !addedFury && !(MainPlugin.enableDragonDuration.Value & dragonForm))
			{
				addedFury = true;
				behaviour.CmdAddFury(furyGain);
			}
			if (!isInHitPause)
			{
				stopwatch += Time.fixedDeltaTime;
			}
			else
			{
				base.characterMotor.velocity = Vector3.zero;
				animator.SetFloat("M1", 0f);
			}
			if (base.isAuthority && stopwatch >= attackDuration - earlyExitDuration)
			{
				if (base.inputBank.skill1.down)
				{
					SetState();
					return;
				}
				if (stopwatch >= attackDuration) // || comboState == 3 && stopwatch >= attackDuration - earlyExitDuration)
				{
					outer.SetNextStateToMain();
					return;
				}
			}
		}
		public virtual void SetState()
		{
			Primary groundLight = new Primary();
			groundLight.comboState = comboState == 3 ? 1 : comboState + 1;
			outer.SetNextState(groundLight);
		}
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
		public override void OnSerialize(NetworkWriter writer)
		{
			base.OnSerialize(writer);
			writer.Write((byte)comboState);
		}
		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
			comboState = (int)reader.ReadByte();
		}
	}
}