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
using HG;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Bindings;

namespace Shyvana
{
    class ShyvSkillDef : SkillDef
    {
        public override SkillDef.BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new ShyvSkillDef.InstanceData
            {
                skillLocator = skillSlot.GetComponent<SkillLocator>(),
                behaviour = skillSlot.GetComponent<ShyvanaBehaviour>()
            };
        }
        internal static bool IsExecutable([NotNull] GenericSkill skillSlot)
        {
            ShyvSkillDef.InstanceData data = ((ShyvSkillDef.InstanceData)skillSlot.skillInstanceData);
            ShyvanaBehaviour behaviour = data.behaviour;
            SkillLocator skillLocator = data.skillLocator;
            CharacterMotor motor = data.motor;
            if (skillLocator && behaviour && behaviour.behaviour)
            {
                if (skillSlot == skillLocator.primary)
                {
                }
                if (skillSlot == skillLocator.secondary)
                {
                }
                if (skillSlot == skillLocator.utility)
                {
                }
                if (!behaviour.behaviour.transformed && skillSlot == skillLocator.special)
                {
                    return behaviour.canExecute && behaviour.powerupValue >= 1.5f;
                }
                return behaviour.canExecute;
            }
            return true;
        }
        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return ShyvSkillDef.IsExecutable(skillSlot) && base.CanExecute(skillSlot);
        }
        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && ShyvSkillDef.IsExecutable(skillSlot);
        }
        class InstanceData : SkillDef.BaseSkillInstanceData
        {
            public SkillLocator skillLocator;
            public ShyvanaBehaviour behaviour;
            public CharacterMotor motor;
        }
    }
}
