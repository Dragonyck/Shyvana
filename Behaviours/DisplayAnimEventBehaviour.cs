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
    class DisplayAnimEventBehaviour : MonoBehaviour
    {
        private uint ID;
        private GameObject wings;
        private void OnEnable()
        {
            ID = AkSoundEngine.PostEvent(Sounds.Play_Shyvana_Recall, base.gameObject);
            wings = base.GetComponent<ChildLocator>().FindChild("wings").gameObject;
        }
        public void EnableWings()
        {
            wings.SetActive(true);
        }
        private void OnDisable()
        {
            AkSoundEngine.StopPlayingID(ID);
        }
    }
}
