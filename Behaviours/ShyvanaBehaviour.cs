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
    class ShyvanaBehaviour : NetworkBehaviour
    {
        public bool canExecute = true;
        private CharacterBody body;
        private CharacterMaster master;
        public MasterBehaviour behaviour;
        public float powerupValue;
        public GameObject effect;
        public uint ID;
        public void AddFury(float value)
        {
            if (behaviour.behaviour)
            {
                behaviour.AddFury(value);
            }
        }
        [Command]
        public void CmdAddFury(float value)
        {
            if (behaviour.behaviour)
            {
                RpcAddFury(value);
            }
        }
        [ClientRpc]
        public void RpcAddFury(float value)
        {
            if (behaviour.behaviour)
            {
                behaviour.AddFury(value);
            }
        }
        [Command]
        public void CmdTransformBody(string bodyName)
        {
            if (master)
            {
                master.TransformBody(bodyName);
            }
        }
        private void OnEnable()
        {
            body = base.GetComponent<CharacterBody>();
            if (body && body.masterObject)
            {
                master = body.master;
                behaviour = body.masterObject.GetComponent<MasterBehaviour>();
                if (behaviour)
                {
                    behaviour.behaviour = this;
                }
            }
        }
        private void FixedUpdate()
        {
            if (body && body.masterObject)
            {
                if (!master)
                {
                    master = body.masterObject.GetComponent<CharacterMaster>();
                }
                if (!behaviour)
                {
                    behaviour = body.masterObject.GetComponent<MasterBehaviour>();
                    if (!behaviour)
                    {
                        behaviour = body.masterObject.AddComponent<MasterBehaviour>();
                        if (behaviour)
                        {
                            behaviour.behaviour = this;
                        }
                    }
                }
                else if (!behaviour.behaviour)
                {
                    behaviour.behaviour = this;
                }
            }
            if (behaviour)
            {
                powerupValue = behaviour.powerupValue;
            }
        }
    }
}
