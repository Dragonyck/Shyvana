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
    class SpecialSelect : BaseShyvanaState
    {
        public Vector3 pos;
        private GameObject areaIndicator;
        public GameObject lineIndicator;
        private Transform lineEndTransform;
        private BezierCurveLine bezier;
        private LineRenderer line;
        private MeshRenderer mesh;
        private bool invalidSelection;

        public override void OnEnter()
        {
            base.OnEnter();
            if (base.isAuthority)
            {
                areaIndicator = UnityEngine.Object.Instantiate(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab, base.gameObject.transform);
                areaIndicator.transform.localScale = Vector3.one * 12;
                mesh = areaIndicator.GetComponentsInChildren<MeshRenderer>(false)[0];

                lineIndicator = UnityEngine.Object.Instantiate(Prefabs.lineIndicator, base.transform.position, Quaternion.identity, base.gameObject.transform);
                lineEndTransform = lineIndicator.transform.GetChild(0);
                bezier = lineIndicator.GetComponent<BezierCurveLine>();
                line = lineIndicator.GetComponent<LineRenderer>();
            }
        }
        public override void Update()
        {
            base.Update();
            if (base.isAuthority && areaIndicator && lineIndicator && bezier && line && mesh)
            {
                //lineIndicator.transform.position = base.transform.position;

                float maxDistance = 280;
                float num = 0f;
                RaycastHit raycastHit;
                if (Physics.Raycast(CameraRigController.ModifyAimRayIfApplicable(base.GetAimRay(), base.gameObject, out num), out raycastHit, maxDistance + num, LayerIndex.world.mask))
                {
                    //areaIndicator.SetActive(true);
                    //lineIndicator.SetActive(true);

                    line.material = Prefabs.baseArcMat;
                    mesh.material = Prefabs.baseIndicatorMat;

                    pos = raycastHit.point;

                    areaIndicator.transform.position = raycastHit.point;
                    areaIndicator.transform.up = raycastHit.normal;

                    if (lineEndTransform)
                    {
                        lineEndTransform.position = raycastHit.point;
                    }

                    invalidSelection = false;
                }
                else
                {
                    invalidSelection = true;
                    line.material = Prefabs.redArcMat;
                    mesh.material = Prefabs.redIndicatorMat;
                }
            }
            if (!base.inputBank.skill4.down && bezier)
            {
                if (pos == Vector3.zero || invalidSelection)
                {
                    base.skillLocator.special.Reset();
                    this.outer.SetNextStateToMain();
                    return;
                }
                behaviour.behaviour.positions = bezier.vertexList;
                this.outer.SetNextState(new Special());
                return;
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (areaIndicator)
            {
                EntityState.Destroy(areaIndicator);
            }
            if (lineIndicator)
            {
                EntityState.Destroy(lineIndicator);
            }
        }
    }
    class Special : BaseShyvanaState
    {
        private float duration = 0.3f;

        public override void OnEnter()
        {
            base.OnEnter();
            base.AdjustModelForwardAim();
            base.PlayAnimation("FullBody, Override", "Spell4", "Spell4", duration);
            AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Transform_Cast, base.gameObject);
        }
        public override void Update()
        {
            base.Update();
            base.characterBody.moveSpeed = 0;
            base.animator.SetBool("isMoving", false);
            base.characterMotor.velocity = Vector3.zero;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (behaviour.behaviour)
            {
                //behaviour.behaviour.forward = base.characterDirection.forward;
            }
            if (NetworkServer.active && master)
            {
                master.TransformBody("DragonShyvanaBody");
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
