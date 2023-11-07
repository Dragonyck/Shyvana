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
    class BaseShyvanaState : BaseSkillState
    {
        public Animator animator;
        public ShyvanaBehaviour behaviour;
        public MasterBehaviour masterBehaviour;
        public CharacterMaster master;
        public override void OnEnter()
        {
            base.OnEnter();
            animator = base.GetModelAnimator();
            behaviour = base.GetComponent<ShyvanaBehaviour>();
            if (behaviour && behaviour.behaviour)
            {
                masterBehaviour = behaviour.behaviour;
            }
            master = base.characterBody.master;
        }
        public void LockSkills()
        {
            if (behaviour)
            {
                behaviour.canExecute = false;
            }
        }
        public void UnlockSkills()
        {
            if (behaviour)
            {
                behaviour.canExecute = true;
            }
        }
        public void AdjustModelForward(Vector3 dir)
        {
            if (base.characterDirection)
            {
                if (dir != Vector3.zero)
                {
                    dir.Normalize();
                    var direction = new Vector3(dir.x, 0f, dir.y).normalized;
                    base.characterDirection.moveVector = direction;
                }
            }
        }
        public void AdjustModelForwardAim()
        {
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
        }
    }
}
