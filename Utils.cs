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
using UnityEngine.AddressableAssets;
using Rewired.ComponentControls.Effects;

namespace Shyvana
{
    class Utils
    {
        public static GenericSkill NewGenericSkill(GameObject obj, SkillDef skill)
        {
            GenericSkill generic = obj.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            generic._skillFamily = newFamily;
            SkillFamily skillFamily = generic.skillFamily;
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = skill,
                viewableNode = new ViewablesCatalog.Node(skill.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);
            return generic;
        }
        public static EntityStateMachine NewStateMachine<T>(GameObject obj, string customName) where T: EntityState
        {
            SerializableEntityStateType s = new SerializableEntityStateType(typeof(T));
            var newStateMachine = obj.AddComponent<EntityStateMachine>();
            newStateMachine.customName = customName;
            newStateMachine.initialStateType = s;
            newStateMachine.mainStateType = s;
            return newStateMachine;
        }
        public static BuffDef NewBuffDef(string name, bool stack, bool hidden, Sprite sprite, Color color)
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.name = name;
            buff.canStack = stack;
            buff.isHidden = hidden;
            buff.iconSprite = sprite;
            buff.buffColor = color;
            ContentAddition.AddBuffDef(buff);
            return buff;
        }
        public static GameObject NewDisplayModel(GameObject model, string name)
        {
            GameObject characterDisplay = PrefabAPI.InstantiateClone(model, name, false);
            characterDisplay.GetComponentInChildren<CharacterModel>().enabled = false;
            characterDisplay.GetComponentInChildren<Animator>().runtimeAnimatorController = Assets.Load<RuntimeAnimatorController>("shyvDisplayAnimator");
            UnityEngine.Object.Destroy(characterDisplay.GetComponentInChildren<ModelSkinController>());
            characterDisplay.transform.GetChild(1).gameObject.AddComponent<DisplayAnimEventBehaviour>();
            var renderers = characterDisplay.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].gameObject.SetActive(true);
                renderers[i].material.shader = Prefabs.Load<Shader>("RoR2/Base/Shaders/HGStandard.shader");
                renderers[i].material.shaderKeywords = null;
            }
            return characterDisplay;
        }
        internal static LoadoutAPI.SkinDefInfo CreateNewSkinDefInfo(SkinnedMeshRenderer[] childList, GameObject rootObject, string skinName, CharacterModel.RendererInfo[] rendererInfos)
        {
            LoadoutAPI.SkinDefInfo skinDefInfo = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            List<SkinDef.GameObjectActivation> objectActivations = new List<SkinDef.GameObjectActivation>();
            foreach (SkinnedMeshRenderer obj in childList)
            {
                SkinDef.GameObjectActivation activation = new SkinDef.GameObjectActivation()
                {
                    gameObject = obj.gameObject,
                    shouldActivate = obj == childList[0]
                };
                objectActivations.Add(activation);
            }
            skinDefInfo.GameObjectActivations = objectActivations.ToArray();
            skinDefInfo.Icon = Assets.MainAssetBundle.LoadAsset<Sprite>("skinPortrait");
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[0];
            skinDefInfo.Name = skinName;
            skinDefInfo.NameToken = skinName;
            skinDefInfo.RendererInfos = rendererInfos;
            skinDefInfo.RootObject = rootObject;
            skinDefInfo.UnlockableDef = null;
            return skinDefInfo;
        }
        public static GameObject CreateHitbox(string name, Transform parent, Vector3 scale)
        {
            var hitboxTransform1 = new GameObject(name);
            hitboxTransform1.transform.SetParent(parent);
            hitboxTransform1.transform.localPosition = Vector3.zero;
            hitboxTransform1.transform.localRotation = Quaternion.identity;
            hitboxTransform1.transform.localScale = scale;
            var hitBoxGroup1 = parent.gameObject.AddComponent<HitBoxGroup>();
            HitBox hitBox = hitboxTransform1.AddComponent<HitBox>();
            hitboxTransform1.layer = LayerIndex.projectile.intVal;
            hitBoxGroup1.hitBoxes = new HitBox[] { hitBox };
            hitBoxGroup1.groupName = name;
            return hitboxTransform1;
        }
        internal static EffectComponent RegisterEffect(GameObject effect, float duration, string soundName = "", bool parentToReferencedTransform = true, bool positionAtReferencedTransform = true)
        {
            var effectcomponent = effect.GetComponent<EffectComponent>();
            if (!effectcomponent)
            {
                effectcomponent = effect.AddComponent<EffectComponent>();
            }                                   
            if (duration != -1)
            {
                var destroyOnTimer = effect.GetComponent<DestroyOnTimer>();
                if (!destroyOnTimer)
                {
                    effect.AddComponent<DestroyOnTimer>().duration = duration;
                }
                else
                {
                    destroyOnTimer.duration = duration;
                }
            }
            if (!effect.GetComponent<NetworkIdentity>())
            {
                effect.AddComponent<NetworkIdentity>();
            }
            if (!effect.GetComponent<VFXAttributes>())
            {
                effect.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            }
            effectcomponent.applyScale = false;
            effectcomponent.effectIndex = EffectIndex.Invalid;
            effectcomponent.parentToReferencedTransform = parentToReferencedTransform;
            effectcomponent.positionAtReferencedTransform = positionAtReferencedTransform;
            effectcomponent.soundName = soundName;
            ContentAddition.AddEffect(effect);
            return effectcomponent;
        }
        public static Material InstantiateMaterial(Texture tex)
        {
            Material mat = UnityEngine.Object.Instantiate<Material>(Prefabs.Load<Material>("RoR2/Base/Commando/matCommandoDualies.mat"));
            if (mat)
            {
                mat.SetColor("_Color", Color.white);
                mat.SetTexture("_MainTex", tex);
                mat.SetColor("_EmColor", Color.black);
                mat.SetFloat("_EmPower", 0);
                mat.SetTexture("_EmTex", null);
                mat.SetFloat("_NormalStrength", 1f);
                mat.SetTexture("_NormalTex", null);
                return mat;
            }
            return mat;
        }
        public static ObjectScaleCurve AddScaleComponent(GameObject target, float timeMax)
        {
            ObjectScaleCurve scale = target.AddComponent<ObjectScaleCurve>();
            scale.useOverallCurveOnly = true;
            scale.timeMax = timeMax;
            scale.overallCurve = AnimationCurve.Linear(0, 0, 1, 1);
            return scale;
        }
    }
}
