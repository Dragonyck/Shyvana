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

namespace Shyvana
{
    class MasterBehaviour : MonoBehaviour
    {
        public CharacterMaster master;
        public ShyvanaBehaviour behaviour;
        public CharacterBody body;
        public float maxPowerupValue = 2;
        public float powerupValue = 2;
        public float stopwatch = 0;
        public float passivePowerupGain
        {
            get
            {
                if (MainPlugin.enableDragonDuration.Value && transformed)
                {
                    return -maxPowerupValue / MainPlugin.minDragonDuration.Value;
                }
                return transformed ? -0.04f : 0.02f;
            }
        }
        public GameObject powerupBar;
        public TextMeshProUGUI currentPowerup;
        public Image barImage;
        public bool transformed;
        public bool despawned;
        public Vector3[] positions;
        public void AddFury(float value)
        {
            powerupValue += value;
        }
        private void Start()
        {
            master = base.GetComponent<CharacterMaster>();
        }
        private void OnEnable()
        {
            On.RoR2.UI.HUD.Update += HUD_Update;
            Stage.onStageStartGlobal += (self) => { powerupBar = null; positions = Array.Empty<Vector3>(); };
        }
        private void OnDisable()
        {
            On.RoR2.UI.HUD.Update -= HUD_Update;
        }
        private void HUD_Update(On.RoR2.UI.HUD.orig_Update orig, HUD self)
        {
            orig(self);
            if (master && self.targetMaster && self.targetMaster == master)
            {
                if (barImage && currentPowerup)
                {
                    barImage.fillAmount = powerupValue / maxPowerupValue;
                    string powerupString = Mathf.Clamp(Mathf.RoundToInt(powerupValue * 100), 0, maxPowerupValue * 100).ToString();
                    currentPowerup.text = powerupString;
                }
                #region barSetup
                if (self.mainUIPanel && !powerupBar)
                {
                    var healthBar = self.mainUIPanel.GetComponentInChildren<HealthBar>();
                    if (healthBar && healthBar.gameObject)
                    {
                        var images = healthBar.gameObject.GetComponentsInChildren<Image>();
                        if (images.Length >= 5 && self.targetBodyObject && Util.HasEffectiveAuthority(self.targetBodyObject))
                        {
                            AddBar(healthBar.gameObject);
                        }
                    }
                }
                #endregion
            }
        }
        private void AddBar(GameObject healthBar)
        {
            powerupBar = Instantiate(healthBar, healthBar.transform.parent);
            powerupBar.name = "PowerupBar";
            Destroy(powerupBar.GetComponent<HealthBar>());
            var texts = powerupBar.GetComponentsInChildren<TextMeshProUGUI>();
            for (int l = 0; l < texts.Length; l++)
            {
                if (texts[l] && texts[l].gameObject)
                {
                    if (texts[l].gameObject.name == "CurrentHealthText")
                    {
                        //texts[l].transform.localPosition = new Vector2(30, 15);
                        currentPowerup = texts[l];
                        currentPowerup.text = "0";

                        //texts[l].transform.parent.GetComponent<TextMeshProUGUI>().text = "";
                    }
                    if (texts[l].gameObject.name == "FullHealthText")
                    {
                        //Destroy(texts[l].gameObject);
                        texts[l].text = (maxPowerupValue * 100).ToString();
                    }
                }
            }
            var powerupImages = powerupBar.GetComponentsInChildren<Image>();
            for (int t = 0; t < powerupImages.Length; t++)
            {
                if (powerupImages[t] && powerupImages[t].gameObject)
                {
                    if (powerupImages[t] != powerupImages[3] && powerupImages[t] != powerupImages[0])
                    {
                        Destroy(powerupImages[t].gameObject);
                    }
                    if (powerupImages[t] == powerupImages[3])
                    {
                        barImage = powerupImages[t];
                        barImage.color = new Color(0.61176f, 0.04314f, 0.07059f);
                        barImage.type = Image.Type.Filled;
                        barImage.fillMethod = Image.FillMethod.Horizontal;
                        barImage.fillCenter = false;
                    }
                }
            }
        }
        private void FixedUpdate()
        {
            if (master && !body)
            {
                body = master.GetBody();
            }
            if (master && body)//body.isPlayerControlled
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= 1)
                {
                    stopwatch = 0;
                    powerupValue += passivePowerupGain;
                }
                powerupValue = Mathf.Clamp(powerupValue, 0, maxPowerupValue);
                if (body && transformed && powerupValue <= 0 && behaviour && behaviour.canExecute)
                {
                    Detransform();
                }
            }
        }
        private void Detransform()
        {
            transformed = false;
            despawned = true; 
            if (body && body.hasEffectiveAuthority)
            {
                behaviour.CmdTransformBody("ShyvanaBody");
            }
        }
    }
}
