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

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace Shyvana
{
    [BepInIncompatibility("com.Wolfo.WolfoQualityOfLife")]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInDependency(R2API.LoadoutAPI.PluginGUID)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    [BepInDependency(R2API.SoundAPI.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2API.DamageAPI.PluginGUID)]
    [BepInPlugin(MODUID, MODNAME, VERSION)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Dragonyck.Shyvana";
        public const string MODNAME = "Shyvana";
        public const string VERSION = "1.0.2";
        public const string SURVIVORNAME = "Shyvana";
        public const string SURVIVORNAMEKEY = "SHYVANA";
        public const string DSURVIVORNAME = "Shyvana (Dragon Form)";
        public const string DSURVIVORNAMEKEY = "DRAGONSHYVANA";
        public static GameObject characterPrefab;
        public static GameObject dragonCharacterPrefab;
        public static readonly Color characterColor = new Color(0.02745f, 0.92549f, 0.61176f);//07ec9c
        protected internal static ConfigEntry<bool> enableDragonDuration;
        protected internal static ConfigEntry<float> minDragonDuration;

        private void Awake()
        {
            enableDragonDuration = base.Config.Bind<bool>(new ConfigDefinition("Enable Dragon Form Minimum Duration", "Value"), false, new ConfigDescription("", null, Array.Empty<object>()));
            minDragonDuration = base.Config.Bind<float>(new ConfigDefinition("Minimum Dragon Form Duration", "Value"), 40, new ConfigDescription("", null, Array.Empty<object>()));

            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (self, user, t) => { };

            Assets.PopulateAssets();
            Prefabs.CreatePrefabs();
            CreatePrefab();
            CreateDragonPrefab();
            RegisterStates();
            RegisterCharacter();
            Hook.Hooks();
        }
        internal static void CreatePrefab()
        {
            var commandoBody = Prefabs.Load<GameObject>("RoR2/Base/Commando/CommandoBody.prefab");

            characterPrefab = PrefabAPI.InstantiateClone(commandoBody, SURVIVORNAME + "Body", true);
            characterPrefab.AddComponent<ShyvanaBehaviour>();
            characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
            Destroy(characterPrefab.transform.Find("ModelBase").gameObject);
            Destroy(characterPrefab.transform.Find("CameraPivot").gameObject);
            Destroy(characterPrefab.transform.Find("AimOrigin").gameObject);

            GameObject model = Assets.MainAssetBundle.LoadAsset<GameObject>("shyvana");

            GameObject ModelBase = new GameObject("ModelBase");
            ModelBase.transform.parent = characterPrefab.transform;
            ModelBase.transform.localPosition = new Vector3(0f, -0.94f, 0f);
            ModelBase.transform.localRotation = Quaternion.identity;
            ModelBase.transform.localScale = Vector3.one * 1.3f;

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = ModelBase.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = ModelBase.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = characterPrefab.GetComponent<CharacterDirection>();
            characterDirection.targetTransform = ModelBase.transform;
            characterDirection.modelAnimator = model.GetComponentInChildren<Animator>();

            CharacterBody bodyComponent = characterPrefab.GetComponent<CharacterBody>();
            bodyComponent.name = SURVIVORNAME + "Body";
            bodyComponent.baseNameToken = SURVIVORNAMEKEY + "_NAME";
            bodyComponent.subtitleNameToken = SURVIVORNAMEKEY + "_SUBTITLE";
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;
            bodyComponent.mainRootSpeed = 0;
            bodyComponent.baseMaxHealth = 160;
            bodyComponent.levelMaxHealth = 48;
            bodyComponent.baseRegen = 2.5f;
            bodyComponent.levelRegen = 0.5f;
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0;
            bodyComponent.baseMoveSpeed = 7;
            bodyComponent.levelMoveSpeed = 0;
            bodyComponent.baseAcceleration = 110;
            bodyComponent.baseJumpPower = 15;
            bodyComponent.levelJumpPower = 0;
            bodyComponent.baseDamage = 12;
            bodyComponent.levelDamage = 2.4f;
            bodyComponent.baseAttackSpeed = 1;
            bodyComponent.levelAttackSpeed = 0;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0;
            bodyComponent.baseArmor = 20;
            bodyComponent.levelArmor = 0;
            bodyComponent.baseJumpCount = 1;
            bodyComponent.sprintingSpeedMultiplier = 1.45f;
            bodyComponent.wasLucky = false;
            bodyComponent.hideCrosshair = false;
            bodyComponent.aimOriginTransform = gameObject3.transform;
            bodyComponent.hullClassification = HullClassification.Human;
            bodyComponent.portraitIcon = Assets.MainAssetBundle.LoadAsset<Sprite>("portrait").texture;
            bodyComponent._defaultCrosshairPrefab = Prefabs.Load<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab");
            bodyComponent.isChampion = false;
            bodyComponent.currentVehicle = null;
            bodyComponent.skinIndex = 0U;
            bodyComponent.bodyColor = characterColor;

            CharacterMotor characterMotor = characterPrefab.GetComponent<CharacterMotor>();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 160f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;

            CameraTargetParams cameraTargetParams = characterPrefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = Prefabs.Load<CharacterCameraParams>("RoR2/Base/Common/ccpStandardMelee.asset");
            cameraTargetParams.cameraPivotTransform = null;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            ModelLocator modelLocator = characterPrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = ModelBase.transform;
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = false;
            modelLocator.normalizeToFloor = false;
            modelLocator.preserveModel = false;

            CharacterModel characterModel = model.AddComponent<CharacterModel>();

            SkinnedMeshRenderer[] renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            List<CharacterModel.RendererInfo> rendererInfoList = new List<CharacterModel.RendererInfo>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var mat = Utils.InstantiateMaterial(renderers[i].material.mainTexture);
                renderers[i].material = mat;
                rendererInfoList.Add(new CharacterModel.RendererInfo()
                {
                    renderer = renderers[i],
                    defaultMaterial = mat,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                });
            }
            CharacterModel.RendererInfo[] rendererInfos = rendererInfoList.ToArray();
            characterModel.body = bodyComponent;
            characterModel.baseRendererInfos = rendererInfos;
            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();
            characterModel.mainSkinnedMeshRenderer = renderers[0];

            model.GetComponent<ChildLocator>().FindChild("wings").gameObject.SetActive(false);

            GameObject gameObject = transform.gameObject;
            ModelSkinController modelSkinController = gameObject.AddComponent<ModelSkinController>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "BODY_DEFAULT_SKIN_NAME", "Ruined");

            modelSkinController.skins = new SkinDef[]
            {
                LoadoutAPI.CreateNewSkinDef(Utils.CreateNewSkinDefInfo(renderers, gameObject, SURVIVORNAMEKEY + "BODY_DEFAULT_SKIN_NAME", rendererInfos))
            };

            HealthComponent healthComponent = characterPrefab.GetComponent<HealthComponent>();
            healthComponent.health = bodyComponent.baseMaxHealth;
            healthComponent.shield = 0f;
            healthComponent.barrier = 0f;
            healthComponent.magnetiCharge = 0f;
            healthComponent.body = null;
            healthComponent.dontShowHealthbar = false;
            healthComponent.globalDeathEventChanceCoefficient = 1f;

            CapsuleCollider[] colliders = model.GetComponentsInChildren<CapsuleCollider>();
            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();
            List<HurtBox> hurtboxes = new List<HurtBox>();
            foreach (CapsuleCollider c in colliders)
            {
                HurtBox hurtbox = c.gameObject.AddComponent<HurtBox>();
                hurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
                hurtbox.healthComponent = healthComponent;
                hurtbox.isBullseye = true;
                hurtbox.damageModifier = HurtBox.DamageModifier.Normal;
                hurtbox.hurtBoxGroup = hurtBoxGroup;
                hurtbox.indexInGroup = 0;

                hurtBoxGroup.mainHurtBox = hurtbox;
                hurtBoxGroup.bullseyeCount = 1;

                hurtboxes.Add(hurtbox);
            }
            hurtBoxGroup.hurtBoxes = hurtboxes.ToArray();

            Utils.CreateHitbox("Swing", model.transform, new Vector3(4.2f, 4.2f, 4.7f)).transform.localPosition = new Vector3(0, 1, 1.5f);

            KinematicCharacterMotor kinematicCharacterMotor = characterPrefab.GetComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;
            //kinematicCharacterMotor.Capsule = colliders[0];=

            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = Prefabs.Load<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab");

            EntityStateMachine mainStateMachine = bodyComponent.GetComponent<EntityStateMachine>();
            mainStateMachine.initialStateType = new SerializableEntityStateType(typeof(ShyvanaSpawnState));
            mainStateMachine.mainStateType = new SerializableEntityStateType(typeof(GenericCharacterMain));

            CharacterDeathBehavior characterDeathBehavior = characterPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
            characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));

            Utils.NewStateMachine<Idle>(characterPrefab, "Burnout");

            NetworkStateMachine networkStateMachine = bodyComponent.GetComponent<NetworkStateMachine>();
            networkStateMachine.stateMachines = bodyComponent.GetComponents<EntityStateMachine>();

            ContentAddition.AddBody(characterPrefab);
        }
        internal static void CreateDragonPrefab()
        {
            var commandoBody = Prefabs.Load<GameObject>("RoR2/Base/Commando/CommandoBody.prefab");

            dragonCharacterPrefab = PrefabAPI.InstantiateClone(commandoBody, "DragonShyvanaBody", true);
            dragonCharacterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
            dragonCharacterPrefab.AddComponent<ShyvanaBehaviour>();
            Destroy(dragonCharacterPrefab.transform.Find("ModelBase").gameObject);
            Destroy(dragonCharacterPrefab.transform.Find("CameraPivot").gameObject);
            Destroy(dragonCharacterPrefab.transform.Find("AimOrigin").gameObject);

            GameObject model = Assets.MainAssetBundle.LoadAsset<GameObject>("dragon");
            model.AddComponent<AnimEventsBehaviour>();

            GameObject ModelBase = new GameObject("ModelBase");
            ModelBase.transform.parent = dragonCharacterPrefab.transform;
            ModelBase.transform.localPosition = new Vector3(0f, -0.94f, 0f);
            ModelBase.transform.localRotation = Quaternion.identity;
            ModelBase.transform.localScale = Vector3.one * 2;

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = ModelBase.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 2.4f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = ModelBase.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = dragonCharacterPrefab.GetComponent<CharacterDirection>();
            characterDirection.targetTransform = ModelBase.transform;
            characterDirection.modelAnimator = model.GetComponentInChildren<Animator>();

            CharacterBody bodyComponent = dragonCharacterPrefab.GetComponent<CharacterBody>();
            bodyComponent.name = "DragonShyvanaBody";
            bodyComponent.baseNameToken = DSURVIVORNAMEKEY + "_NAME";
            bodyComponent.subtitleNameToken = DSURVIVORNAMEKEY + "_SUBTITLE";
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;
            bodyComponent.mainRootSpeed = 0;
            bodyComponent.baseMaxHealth = 160 + 250;
            bodyComponent.levelMaxHealth = 48;
            bodyComponent.baseRegen = 2.5f;
            bodyComponent.levelRegen = 0.5f;
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0;
            bodyComponent.baseMoveSpeed = 7;
            bodyComponent.levelMoveSpeed = 0;
            bodyComponent.baseAcceleration = 110;
            bodyComponent.baseJumpPower = 20;
            bodyComponent.levelJumpPower = 0;
            bodyComponent.baseDamage = 12;
            bodyComponent.levelDamage = 2.4f;
            bodyComponent.baseAttackSpeed = 1;
            bodyComponent.levelAttackSpeed = 0;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0;
            bodyComponent.baseArmor = 20;
            bodyComponent.levelArmor = 3;
            bodyComponent.baseJumpCount = 99;
            bodyComponent.sprintingSpeedMultiplier = 1.45f;
            bodyComponent.wasLucky = false;
            bodyComponent.hideCrosshair = false;
            bodyComponent.aimOriginTransform = gameObject3.transform;
            bodyComponent.hullClassification = HullClassification.Human;
            bodyComponent.portraitIcon = Assets.MainAssetBundle.LoadAsset<Sprite>("portraitDragon").texture;
            bodyComponent._defaultCrosshairPrefab = Prefabs.Load<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab");
            bodyComponent.isChampion = false;
            bodyComponent.currentVehicle = null;
            bodyComponent.skinIndex = 0U;
            bodyComponent.bodyColor = characterColor;

            CharacterMotor characterMotor = dragonCharacterPrefab.GetComponent<CharacterMotor>();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 450f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;

            CameraTargetParams cameraTargetParams = dragonCharacterPrefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = Prefabs.Load<CharacterCameraParams>("RoR2/Base/Common/ccpStandardMelee.asset");
            cameraTargetParams.cameraPivotTransform = null;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            ModelLocator modelLocator = dragonCharacterPrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = ModelBase.transform;
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = false;
            modelLocator.normalizeToFloor = false;
            modelLocator.preserveModel = false;

            dragonCharacterPrefab.GetComponent<Interactor>().maxInteractionDistance = 6;

            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            CharacterModel characterModel = model.AddComponent<CharacterModel>();

            SkinnedMeshRenderer[] renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            List<CharacterModel.RendererInfo> rendererInfoList = new List<CharacterModel.RendererInfo>();
            for (int i = 0; i < renderers.Length; i++)
            {
                rendererInfoList.Add(new CharacterModel.RendererInfo()
                {
                    renderer = renderers[i],
                    defaultMaterial = Utils.InstantiateMaterial(renderers[i].material.mainTexture),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                });
            }
            CharacterModel.RendererInfo[] rendererInfos = rendererInfoList.ToArray();
            characterModel.body = bodyComponent;
            characterModel.baseRendererInfos = rendererInfos;
            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();
            characterModel.mainSkinnedMeshRenderer = renderers[0];

            HealthComponent healthComponent = dragonCharacterPrefab.GetComponent<HealthComponent>();
            healthComponent.health = bodyComponent.baseMaxHealth;
            healthComponent.shield = 0f;
            healthComponent.barrier = 0f;
            healthComponent.magnetiCharge = 0f;
            healthComponent.body = null;
            healthComponent.dontShowHealthbar = false;
            healthComponent.globalDeathEventChanceCoefficient = 1f;

            CapsuleCollider[] colliders = model.GetComponentsInChildren<CapsuleCollider>();
            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();
            List<HurtBox> hurtboxes = new List<HurtBox>();
            foreach (CapsuleCollider c in colliders)
            {
                HurtBox hurtbox = c.gameObject.AddComponent<HurtBox>();
                hurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
                hurtbox.healthComponent = healthComponent;
                hurtbox.isBullseye = true;
                hurtbox.damageModifier = HurtBox.DamageModifier.Normal;
                hurtbox.hurtBoxGroup = hurtBoxGroup;
                hurtbox.indexInGroup = 0;

                hurtBoxGroup.mainHurtBox = hurtbox;
                hurtBoxGroup.bullseyeCount = 1;

                hurtboxes.Add(hurtbox);
            }
            hurtBoxGroup.hurtBoxes = hurtboxes.ToArray();

            model.AddComponent<AimAnimator>();

            Utils.CreateHitbox("Swing", model.transform, new Vector3(6.2f, 6.2f, 6.7f)).transform.localPosition = new Vector3(0, 1, 1.5f);

            KinematicCharacterMotor kinematicCharacterMotor = dragonCharacterPrefab.GetComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;
            //kinematicCharacterMotor.Capsule = colliders[0];=

            dragonCharacterPrefab.GetComponent<SfxLocator>().landingSound = "Play_gravekeeper_land";

            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_parent_step";//Play_beetle_queen_step
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = Prefabs.Load<GameObject>("RoR2/Base/Common/VFX/GenericHugeFootstepDust.prefab");

            EntityStateMachine mainStateMachine = bodyComponent.GetComponent<EntityStateMachine>();
            mainStateMachine.initialStateType = new SerializableEntityStateType(typeof(DragonSpawnState));
            mainStateMachine.mainStateType = new SerializableEntityStateType(typeof(CharacterMain));

            CharacterDeathBehavior characterDeathBehavior = dragonCharacterPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = dragonCharacterPrefab.GetComponent<EntityStateMachine>();
            characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));

            NetworkStateMachine networkStateMachine = bodyComponent.GetComponent<NetworkStateMachine>();
            networkStateMachine.stateMachines = bodyComponent.GetComponents<EntityStateMachine>();

            ContentAddition.AddBody(dragonCharacterPrefab);
        }
        private void RegisterCharacter()
        {
            string desc = "" +
                "<style=cSub>\r\n\r\n< ! > Fury gain from Twin Bite is a flat amount, regardless of the number of enemies hit."
                + Environment.NewLine +
                "<style=cSub>\r\n\r\n< ! > Burnout's health drain can get you out of tough situations as well as keep you healthy in the thick of fights, use it wisely."
                + Environment.NewLine +
                "<style=cSub>\r\n\r\n< ! > You have a nearly infinite amount of jumps in dragon form and you can cancel Draconic Leap into Exhale Fire, allowing for extremely versatile mobility while transformed."
                + Environment.NewLine +
                "<style=cSub>\r\n\r\n< ! > The damage per tick for Exhale Fire scales with your attack speed.";

            string outro = "..and so she left, to decimate other lands with her ire.";
            string fail = "..and so she vanished, her fury finally extinguished.";

            LanguageAPI.Add(SURVIVORNAMEKEY + "_NAME", SURVIVORNAME);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_DESCRIPTION", desc);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SUBTITLE", "");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_OUTRO", outro);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_FAIL", fail);

            var survivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            {
                survivorDef.cachedName = SURVIVORNAMEKEY + "_NAME";
                survivorDef.unlockableDef = null;
                survivorDef.descriptionToken = SURVIVORNAMEKEY + "_DESCRIPTION";
                survivorDef.primaryColor = characterColor;
                survivorDef.bodyPrefab = characterPrefab;
                survivorDef.displayPrefab = Utils.NewDisplayModel(characterPrefab.GetComponent<ModelLocator>().modelBaseTransform.gameObject, SURVIVORNAME + "Display");
                survivorDef.outroFlavorToken = SURVIVORNAMEKEY + "_OUTRO";
                survivorDef.desiredSortPosition = 32f;
                survivorDef.mainEndingEscapeFailureFlavorToken = SURVIVORNAMEKEY + "_FAIL";
            };
            ContentAddition.AddSurvivorDef(survivorDef);

            survivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            {
                survivorDef.cachedName = DSURVIVORNAMEKEY + "_NAME";
                survivorDef.unlockableDef = null;
                survivorDef.descriptionToken = SURVIVORNAMEKEY + "_DESCRIPTION";
                survivorDef.primaryColor = characterColor;
                survivorDef.bodyPrefab = dragonCharacterPrefab;
                survivorDef.displayPrefab = Utils.NewDisplayModel(dragonCharacterPrefab.GetComponent<ModelLocator>().modelBaseTransform.gameObject, DSURVIVORNAME + "Display");
                survivorDef.outroFlavorToken = SURVIVORNAMEKEY + "_OUTRO";
                survivorDef.mainEndingEscapeFailureFlavorToken = SURVIVORNAMEKEY + "_FAIL";
                survivorDef.desiredSortPosition = 32.1f;
                survivorDef.hidden = true;
            };
            ContentAddition.AddSurvivorDef(survivorDef);

            SkillSetup();
            DragonSkillSetup();

            var characterMaster = PrefabAPI.InstantiateClone(Prefabs.Load<GameObject>("RoR2/Base/Commando/CommandoMonsterMaster.prefab"), SURVIVORNAME + "Master", true);

            ContentAddition.AddMaster(characterMaster);

            CharacterMaster component = characterMaster.GetComponent<CharacterMaster>();
            component.bodyPrefab = characterPrefab;
        }
        void RegisterStates()
        {
            bool hmm;
            ContentAddition.AddEntityState<ShyvanaSpawnState>(out hmm);
            ContentAddition.AddEntityState<BaseShyvanaState>(out hmm);
            ContentAddition.AddEntityState<Primary>(out hmm);
            ContentAddition.AddEntityState<DragonPrimary>(out hmm);
            ContentAddition.AddEntityState<Secondary>(out hmm);
            ContentAddition.AddEntityState<DragonSecondary>(out hmm);
            ContentAddition.AddEntityState<DragonSecondaryEnd>(out hmm);
            ContentAddition.AddEntityState<Utility>(out hmm);
            ContentAddition.AddEntityState<DragonUtility>(out hmm);
            ContentAddition.AddEntityState<SpecialSelect>(out hmm);
            ContentAddition.AddEntityState<Special>(out hmm);
            ContentAddition.AddEntityState<DragonSpecial>(out hmm);
            ContentAddition.AddEntityState<CharacterMain>(out hmm);

            ContentAddition.AddEntityState<DragonSpawnState>(out hmm);
        }
        void SkillSetup()
        {
            foreach (GenericSkill obj in characterPrefab.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }
            PassiveSetup(characterPrefab);
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();
        }
        void PassiveSetup(GameObject prefab)
        {
            SkillLocator component = prefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("DRAGONFORM_KEYWORD", "[ Dragon Form ]");

            LanguageAPI.Add(SURVIVORNAMEKEY + "_PASSIVE_NAME", "Blood of the Dragonborn");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_PASSIVE_DESCRIPTION", "Your draconic bloodline allows you to shapeshift <color=#9C0B12>upon exceeding the Fury threshold</color>. You gain <color=#9C0B12>2 Fury</color> per second in human form and <color=#9C0B12>3 Fury</color> on hitting enemies with Twin Bite. While transformed you drain <color=#9C0B12>4 Fury</color> per second.");

            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = SURVIVORNAMEKEY + "_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = SURVIVORNAMEKEY + "_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("shyvanareinforcedscales");
        }
        void DragonSkillSetup()
        {
            foreach (GenericSkill obj in dragonCharacterPrefab.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }
            PassiveSetup(dragonCharacterPrefab);
            DragonPrimarySetup();
            DragonSecondarySetup();
            DragonUtilitySetup();
            DragonSpecialSetup();
        }
        void DragonPrimarySetup()
        {
            SkillLocator component = dragonCharacterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(DSURVIVORNAMEKEY + "_M1", "Twin Bite");
            LanguageAPI.Add(DSURVIVORNAMEKEY + "_M1_DESCRIPTION", "Cleave enemies in front of you for <style=cIsDamage>350% damage</style>. <color=#9C0B12>Grants +2 Fury on-hit</color>.");

            var SkillDef = ScriptableObject.CreateInstance<ShyvSkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(DragonPrimary));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("shyvanatwinbite");
            SkillDef.skillDescriptionToken = DSURVIVORNAMEKEY + "_M1_DESCRIPTION";
            SkillDef.skillName = DSURVIVORNAMEKEY + "_M1";
            SkillDef.skillNameToken = DSURVIVORNAMEKEY + "_M1";

            ContentAddition.AddSkillDef(SkillDef);

            component.primary = Utils.NewGenericSkill(dragonCharacterPrefab, SkillDef);
        }
        void DragonSecondarySetup()
        {
            SkillLocator component = dragonCharacterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(DSURVIVORNAMEKEY + "_M2", "Exhale Fire");
            LanguageAPI.Add(DSURVIVORNAMEKEY + "_M2_DESCRIPTION", "Breathe fire in a cone in front of you for <style=cIsDamage>50% damage</style>.");

            var SkillDef = ScriptableObject.CreateInstance<ShyvSkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(DragonSecondary));
            SkillDef.activationStateMachineName = "Slide";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 1f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("Incinerate");
            SkillDef.skillDescriptionToken = DSURVIVORNAMEKEY + "_M2_DESCRIPTION";
            SkillDef.skillName = DSURVIVORNAMEKEY + "_M2";
            SkillDef.skillNameToken = DSURVIVORNAMEKEY + "_M2";

            ContentAddition.AddSkillDef(SkillDef);

            component.secondary = Utils.NewGenericSkill(dragonCharacterPrefab, SkillDef);
        }
        void DragonUtilitySetup()
        {
            SkillLocator component = dragonCharacterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add("SHYVANARUIN_KEYWORD", "<style=cKeywordName>Ruin</style><style=cSub>Ruined areas deal <style=cIsDamage>2240% damage over time</style>.");
            LanguageAPI.Add(DSURVIVORNAMEKEY + "_UTIL", "Draconic Leap");
            LanguageAPI.Add(DSURVIVORNAMEKEY + "_UTIL_DESCRIPTION", "Flap your wings and leap in a direction, dealing <style=cIsDamage>320% damage</style>, and <color=#5EE3AA>Ruining</color> an area on impact.");

            var SkillDef = ScriptableObject.CreateInstance<ShyvSkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(DragonUtility));
            SkillDef.activationStateMachineName = "Body";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 5f;
            SkillDef.beginSkillCooldownOnSkillEnd = false;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = false;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("Pillar_of_Flame");
            SkillDef.skillDescriptionToken = DSURVIVORNAMEKEY + "_UTIL_DESCRIPTION";
            SkillDef.skillName = DSURVIVORNAMEKEY + "_UTIL";
            SkillDef.skillNameToken = DSURVIVORNAMEKEY + "_UTIL";
            SkillDef.keywordTokens = new string[] { "SHYVANARUIN_KEYWORD" };

            ContentAddition.AddSkillDef(SkillDef);

            component.utility = Utils.NewGenericSkill(dragonCharacterPrefab, SkillDef);
        }
        void DragonSpecialSetup()
        {
            SkillLocator component = dragonCharacterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add(DSURVIVORNAMEKEY + "_SPEC", "Scorched Earth");
            LanguageAPI.Add(DSURVIVORNAMEKEY + "_SPEC_DESCRIPTION", "Spit a fireball which flies in an arc and explodes for <style=cIsDamage>700% damage,</style> <color=#5EE3AA>Ruining</color> the area of impact.");

            var SkillDef = ScriptableObject.CreateInstance<ShyvSkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(DragonSpecial));
            SkillDef.activationStateMachineName = "Body";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 8f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("Disintegrate");
            SkillDef.skillDescriptionToken = DSURVIVORNAMEKEY + "_SPEC_DESCRIPTION";
            SkillDef.skillName = DSURVIVORNAMEKEY + "_SPEC";
            SkillDef.skillNameToken = DSURVIVORNAMEKEY + "_SPEC";
            SkillDef.keywordTokens = new string[] { "SHYVANARUIN_KEYWORD" };
            ContentAddition.AddSkillDef(SkillDef);

            component.special = Utils.NewGenericSkill(dragonCharacterPrefab, SkillDef);
        }
        void PrimarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1", "Twin Bite");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1_DESCRIPTION", "Swing your gauntlets for <style=cIsDamage>285% damage</style>.  <color=#9C0B12>Grants +2 Fury on-hit</color>.");

            var SkillDef = ScriptableObject.CreateInstance<ShyvSkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Primary));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("shyvanatwinbite");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_M1_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_M1";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_M1";
            SkillDef.keywordTokens = new string[] { "DRAGONFORM_KEYWORD", DSURVIVORNAMEKEY + "_M1", DSURVIVORNAMEKEY + "_M1_DESCRIPTION" };

            ContentAddition.AddSkillDef(SkillDef);

            component.primary = Utils.NewGenericSkill(characterPrefab, SkillDef);
        }
        void SecondarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2", "Flame Breath");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2_DESCRIPTION", "Shoot a slow-moving fireball that <style=cIsDamage>explodes</style> on impact for <style=cIsDamage>315% damage</style>, and <style=cIsDamage>ignites</style> enemies.");

            var SkillDef = ScriptableObject.CreateInstance<ShyvSkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Secondary));
            SkillDef.activationStateMachineName = "Slide";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 5f;
            SkillDef.beginSkillCooldownOnSkillEnd = false;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("shyvanaflamebreath");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_M2_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_M2";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_M2";
            SkillDef.keywordTokens = new string[] { "DRAGONFORM_KEYWORD", DSURVIVORNAMEKEY + "_M2", DSURVIVORNAMEKEY + "_M2_DESCRIPTION" };

            ContentAddition.AddSkillDef(SkillDef);

            component.secondary = Utils.NewGenericSkill(characterPrefab, SkillDef);
        }
        void UtilitySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL", "Burnout");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL_DESCRIPTION", "Wreath yourself in flames and damage enemies for <style=cIsDamage>750% damage</style> per tick, <style=cIsHealth>draining their health</style>. Gain increased movement speed while the flames are active.");

            var SkillDef = ScriptableObject.CreateInstance<ShyvSkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Utility));
            SkillDef.activationStateMachineName = "Burnout";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 5f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = false;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("shyvanascorchedearth");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_UTIL_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_UTIL";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_UTIL";
            SkillDef.keywordTokens = new string[] { "DRAGONFORM_KEYWORD", DSURVIVORNAMEKEY + "_UTIL", DSURVIVORNAMEKEY + "_UTIL_DESCRIPTION", "SHYVANARUIN_KEYWORD" };

            ContentAddition.AddSkillDef(SkillDef);

            component.utility = Utils.NewGenericSkill(characterPrefab, SkillDef);
        }
        void SpecialSetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add(SURVIVORNAMEKEY + "_SPEC", "Dragon's Descent");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SPEC_DESCRIPTION", "<color=#9C0B12>Usable at >150 Fury.</color> Leap to target location, dealing <style=cIsDamage>380% damage</style> on impact and assuming your true form, <style=cIsUtility>changing all of your abilities</style>.");

            var SkillDef = ScriptableObject.CreateInstance<ShyvSkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(SpecialSelect));
            SkillDef.activationStateMachineName = "Slide";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 8f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("shyvanadragonsdescent");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_SPEC_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_SPEC";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_SPEC";
            SkillDef.keywordTokens = new string[] { "DRAGONFORM_KEYWORD", DSURVIVORNAMEKEY + "_SPEC", DSURVIVORNAMEKEY + "_SPEC_DESCRIPTION", "SHYVANARUIN_KEYWORD" };
            ContentAddition.AddSkillDef(SkillDef);

            component.special = Utils.NewGenericSkill(characterPrefab, SkillDef);
        }
    }
}
