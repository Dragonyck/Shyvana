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
	class DragonPrimary : Primary
	{
		public override float comboDamageCoefficient => 3.5f;
        public override bool dragonForm => true;
        public override void SetState()
		{
			DragonPrimary groundLight = new DragonPrimary();
			groundLight.comboState = comboState == 2 ? 1 : comboState + 1;
			outer.SetNextState(groundLight);
		}
        public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}
}