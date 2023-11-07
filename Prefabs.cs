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
using UnityEngine.Rendering.PostProcessing;

namespace Shyvana
{
    class Prefabs
    {
        internal static GameObject fireProjectile;
        internal static GameObject fireProjectileGhost;
        internal static GameObject fireProjectileExplosion;
        internal static GameObject fireBallProjectile;
        internal static GameObject fireBallProjectileGhost;
        internal static GameObject fireBallProjectileExplosion;
        internal static GameObject fireBallProjectileDotZone;
        internal static GameObject flameBreathEffect;
        internal static GameObject ringEffect;
        internal static GameObject despawnEffect;
        internal static GameObject swingHitEffect;
        internal static GameObject dragonSwingHitEffect;
        internal static GameObject dragonSlamEffect;
        internal static GameObject dragonTransformEffect;
        internal static GameObject fireImmolateHitEffect;
        internal static GameObject immolateEffect;
        internal static GameObject lineIndicator;
        internal static GameObject transformIndicator;
        internal static GameObject swingEffect;
        internal static GameObject leapEffect;
        internal static BuffDef flameBuff;
        internal static Material baseIndicatorMat;
        internal static Material redIndicatorMat;
        internal static Material baseArcMat;
        internal static Material redArcMat;
        internal static T Load<T>(string path)
        {
            return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
        }
        internal static void CreatePrefabs()//matVoltaileBatteryExplosion 
        {
            Material matOmniExplosion1Engi = Load<Material>("RoR2/Base/Engi/matOmniExplosion1Engi.mat");
            Material matEngiExplosion = Load<Material>("RoR2/Base/Engi/matEngiExplosion.mat");
            Material matElitePoisonDarkTrail = Load<Material>("RoR2/Base/ElitePoison/matElitePoisonDarkTrail.mat");
            Material matElitePoisonOverlay = Load<Material>("RoR2/Base/ElitePoison/matElitePoisonOverlay.mat");
            Material matElitePoisonParticleReplacement = Load<Material>("RoR2/Base/ElitePoison/matElitePoisonParticleReplacement.mat");
            Material matEngiHarpoonRing = Load<Material>("RoR2/Base/Engi/matEngiHarpoonRing.mat");
            Material matEngiTrailExplosion = Load<Material>("RoR2/Base/Engi/matEngiTrailExplosion.mat");

            baseIndicatorMat = UnityEngine.Object.Instantiate(Load<Material>("RoR2/Base/Common/VFX/matAreaIndicatorRim.mat"));
            baseIndicatorMat.SetColor("_TintColor", MainPlugin.characterColor);

            redIndicatorMat = UnityEngine.Object.Instantiate(baseIndicatorMat);
            redIndicatorMat.SetColor("_TintColor", new Color(1, 0, 0, 0.75f));

            baseArcMat = UnityEngine.Object.Instantiate(Load<Material>("RoR2/Base/Common/VFX/matArcVisual.mat"));
            baseArcMat.SetColor("_TintColor", MainPlugin.characterColor);

            redArcMat = UnityEngine.Object.Instantiate(baseArcMat);
            redArcMat.SetColor("_TintColor", new Color(1, 0, 0, 0.75f));

            flameBuff = Utils.NewBuffDef("Burnout", false, false, Assets.Load<Sprite>("shyvanaBuff"), Color.white);

            swingEffect = PrefabAPI.InstantiateClone(Assets.Load<GameObject>("SlashEffect"), "ShyvanaSlashEffect", false);
            Utils.RegisterEffect(swingEffect, 1, "").applyScale = true;

            leapEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Brother/BrotherDashEffect.prefab"), "ShyvanaLeapEffect", false);
            leapEffect.transform.rotation = Quaternion.Euler(-90, 0, 0);
            foreach (ParticleSystemRenderer r in leapEffect.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var name = r.name;
                if (name == "Donut")
                {
                    r.materials = new Material[] { matEngiTrailExplosion, matEngiTrailExplosion, matEngiTrailExplosion, matEngiTrailExplosion, matEngiTrailExplosion, matEngiTrailExplosion,
                    matEngiTrailExplosion, matEngiTrailExplosion, matEngiTrailExplosion, matEngiTrailExplosion};
                }
                if (name == "Dash")
                {
                    r.material = matEngiExplosion;
                }
            }
            Utils.RegisterEffect(leapEffect, -1);

            dragonTransformEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandParentSunSpawn.prefab"), "ShyvanaDragonTransformEffect", false);
            UnityEngine.Object.Destroy(dragonTransformEffect.GetComponentInChildren<LightIntensityCurve>());
            dragonTransformEffect.GetComponentInChildren<PostProcessVolume>().profile = Load<PostProcessProfile>("RoR2/Base/title/ppLocalArtifactShellHurt.asset");
            var transformLight = dragonTransformEffect.GetComponentInChildren<Light>();
            transformLight.intensity = 45;
            transformLight.range = 50;
            transformLight.colorTemperature = 5000;
            transformLight.color = Color.cyan;
            foreach (ParticleSystemRenderer r in dragonTransformEffect.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var name = r.name;
                if (name == "Goo, Drip" || name == "Trails")
                {
                    r.enabled = false;
                }
                if (name == "Goo, Billboard Huge")
                {
                    r.material = matOmniExplosion1Engi;
                }
                if (name == "Goo, Burst")
                {
                    r.material = Load<Material>("RoR2/Junk/ClayMan/matClaySwingtrail.mat");
                }
                if (name == "Donut")
                {
                    r.material = matEngiTrailExplosion;
                }
            }
            Utils.RegisterEffect(dragonTransformEffect, 1);

            transformIndicator = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Huntress/HuntressArrowRainIndicator.prefab"), "SphereIndicator", false);
            var indicatorMat = UnityEngine.Object.Instantiate(Load<Material>("RoR2/Base/Common/VFX/matAreaIndicatorRim.mat"));
            indicatorMat.SetColor("_TintColor", MainPlugin.characterColor);
            transformIndicator.GetComponentInChildren<MeshRenderer>().material = indicatorMat;

            lineIndicator = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab"), "LineIndicator", false);
            var line = lineIndicator.GetComponent<LineRenderer>();
            line.widthMultiplier = 1;
            line.textureMode = LineTextureMode.Stretch;
            line.numPositions = 98;
            var curve = lineIndicator.AddComponent<BezierCurveLineScaler>();
            curve.v0 = new Vector3(0, 20, 0);
            var transf = new GameObject("EndTransform");
            transf.transform.SetParent(lineIndicator.transform);
            curve.endTransform = transf.transform;

            immolateEffect = PrefabAPI.InstantiateClone(Assets.Load<GameObject>("ImmolateEffect"), "ShyvanaImmolateEffect", false);
            Utils.AddScaleComponent(immolateEffect.transform.GetChild(0).gameObject, 0.15f).overallCurve = AnimationCurve.Linear(0, 0.7f, 1, 1);
            immolateEffect.AddComponent<DestroyOnTimer>().duration = 8;

            fireImmolateHitEffect = PrefabAPI.InstantiateClone(Prefabs.Load<GameObject>("RoR2/Base/Engi/ImpactEngiTurret.prefab"), "ShyvanaDragonImmolateHitEffect", false);
            foreach (ParticleSystemRenderer r in fireImmolateHitEffect.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                r.material.SetColor("_TintColor", MainPlugin.characterColor);
            }
            Utils.RegisterEffect(fireImmolateHitEffect, 1, "Play_Shyvana_Immolate_Hit");

            dragonSlamEffect = PrefabAPI.InstantiateClone(Prefabs.Load<GameObject>("RoR2/Base/Parent/ParentSlamEffect.prefab"), "ShyvanaDragonSlamEffect", false);
            Utils.RegisterEffect(dragonSlamEffect, 1, "Play_Shyvana_Transform_Hit");

            swingHitEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab"), "ShyvanaSwingHitEffect", false);
            UnityEngine.Object.Destroy(swingHitEffect.GetComponent<OmniEffect>());
            foreach (ParticleSystemRenderer r in swingHitEffect.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                r.gameObject.SetActive(true);
                r.material.SetColor("_TintColor", Color.green);
            }
            Utils.RegisterEffect(swingHitEffect, 1, "Play_Shyvana_Auto_Hit");

            dragonSwingHitEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Merc/MercExposeConsumeEffect.prefab"), "ShyvanaDragonSwingHitEffect", false);
            UnityEngine.Object.Destroy(dragonSwingHitEffect.GetComponent<OmniEffect>());
            foreach (ParticleSystemRenderer r in dragonSwingHitEffect.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                r.gameObject.SetActive(true);
                r.material.SetColor("_TintColor", Color.green);
                if (r.name == "PulseEffect, Ring (1)")
                {
                    var mat = r.material;
                    mat.mainTexture = Load<Texture2D>("RoR2/Base/Common/VFX/texArcaneCircleProviMask.png");
                }
            }
            Utils.RegisterEffect(dragonSwingHitEffect, 1, "Play_Shyvana_Dragon_Auto_Hit");

            despawnEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/ImpBoss/ImpBossBlink.prefab"), "ShyvanaDespawnEffect", false);
            UnityEngine.Object.Destroy(despawnEffect.GetComponent<DestroyOnParticleEnd>());
            despawnEffect.GetComponentInChildren<Light>(true).color = MainPlugin.characterColor;
            despawnEffect.GetComponentInChildren<PostProcessVolume>(true).profile = Load<PostProcessProfile>("RoR2/Base/title/ppLocalElectricWorm.asset");
            foreach (ParticleSystemRenderer r in despawnEffect.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var name = r.name;
                if (name == "Dash")
                {
                    r.material = matEngiExplosion;
                }
                if (name == "Dash, Bright")
                {
                    r.material = matEngiTrailExplosion;
                }
                if (name == "DashRings" || name == "Sphere" || name == "LongLifeNoiseTrails")
                {
                    r.material = matOmniExplosion1Engi;
                }
                if (name == "LongLifeNoiseTrails, Bright")
                {
                    r.material = matElitePoisonParticleReplacement;
                }
                if (name == "Flash, Red")
                {
                    r.material = matEngiHarpoonRing;
                }
            }
            Utils.RegisterEffect(despawnEffect, -1, "");

            ringEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Icicle/IcicleAura.prefab"), "ShyvanaRingEffect", false);
            UnityEngine.Object.Destroy(ringEffect.GetComponent<IcicleAuraController>());
            UnityEngine.Object.Destroy(ringEffect.GetComponent<BuffWard>());
            UnityEngine.Object.Destroy(ringEffect.GetComponent<TeamFilter>());
            UnityEngine.Object.Destroy(ringEffect.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(ringEffect.GetComponent<AkGameObj>());
            ringEffect.transform.localScale = Vector3.one * 9;
            ringEffect.transform.GetChild(0).localScale = Vector3.one;
            foreach (ParticleSystem p in ringEffect.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = p.main;
                main.playOnAwake = true;
                //main.loop = true;
                var r = p.GetComponent<ParticleSystemRenderer>();
                var name = r.name;
                if (name == "Ring, Core")
                {
                    r.material = matElitePoisonDarkTrail;
                }
                if (name == "Ring, Outer")
                {
                    r.material = matElitePoisonOverlay;
                }
                if (name == "Ring, Procced")
                {
                    r.material = matElitePoisonDarkTrail;
                }
                if (name == "Area")
                {
                    r.material = matElitePoisonOverlay;
                }
                if (name == "SpinningSharpChunks" || name == "  Chunks")
                {
                    r.enabled = false;
                }
            }

            flameBreathEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Lemurian/FlamebreathEffect.prefab"), "ShyvanaFlameBreathEffect", false);
            flameBreathEffect.GetComponentInChildren<Light>().color = new Color(0.24706f, 0.74902f, 0.50980f);
            flameBreathEffect.GetComponent<ScaleParticleSystemDuration>().newDuration = 1;
            foreach (ParticleSystemRenderer p in flameBreathEffect.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var system = p.GetComponent<ParticleSystem>();
                var main = system.main;
                main.loop = true;
                var emission = system.emission;
                emission.rateOverTime = 55;
                p.material.SetColor("_TintColor", MainPlugin.characterColor);
            }

            fireProjectileExplosion = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Engi/EngiMineExplosion.prefab"), "ShyvanaFireProjectileExplosion", false);
            Utils.RegisterEffect(fireProjectileExplosion, 1, "Play_Shyvana_Fireball_Hit").applyScale = true;

            Material matLunarExploderDeathDecal = UnityEngine.Object.Instantiate(Load<Material>("RoR2/Base/LunarExploder/matLunarExploderDeathDecal.mat"));
            matLunarExploderDeathDecal.SetColor("_Color", MainPlugin.characterColor);
            fireBallProjectileDotZone = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/LunarExploder/LunarExploderProjectileDotZone.prefab"), "ShyvanaFireBallProjectileDotZone", true);
            fireBallProjectileDotZone.GetComponentInChildren<ThreeEyedGames.Decal>().Material = Load<Material>("RoR2/DLC1/AcidLarva/matAcidLarvaDeathDecal.mat");
            fireBallProjectileDotZone.GetComponentInChildren<Light>().color = Color.green;
            foreach (ParticleSystemRenderer p in fireBallProjectileDotZone.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var name = p.name;
                if (name == "Fire, Billboard" || name == "Fire, Stretched")
                {
                    p.material = matOmniExplosion1Engi;
                }
                if (name == "Spores")
                {
                    p.material = Load<Material>("RoR2/Base/Croco/matCrocoGooLarge.mat");
                }
            }
            ContentAddition.AddProjectile(fireBallProjectileDotZone);

            fireBallProjectileExplosion = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/BFG/BeamSphereExplosion.prefab"), "ShyvanaFireBallProjectileExplosion", false);
            foreach (ParticleSystemRenderer p in fireBallProjectileExplosion.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var name = p.name;
                if (name == "Chunks, Sharp" || name == "Lightning" || name == "Ring")
                {
                    p.material = matElitePoisonDarkTrail;
                }
                if (name == "Flames")
                {
                    p.material = matOmniExplosion1Engi;
                }
            }
            Utils.RegisterEffect(fireBallProjectileExplosion, 2, "Play_Shyvana_Dragon_Fireball_Hit");

            fireBallProjectileGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBombGhost.prefab"), "ShyvanaFireBallProjectileGhost", false);
            fireBallProjectileGhost.GetComponentInChildren<Light>().color = Color.green;
            foreach (ParticleSystemRenderer p in fireBallProjectileGhost.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var name = p.name;
                if (name == "BombOrb")
                {
                    p.transform.localScale = Vector3.one * 2;
                    p.material = matEngiTrailExplosion;
                }
                if (name == "Trail_Ps")
                {
                    var system = p.GetComponent<ParticleSystem>();
                    var main = system.main;
                    main.startLifetimeMultiplier = 0.1f;
                    var shape = system.shape;
                    shape.radius *= 0.2f;
                    p.transform.localScale = Vector3.one * 8;
                    p.material = matOmniExplosion1Engi;
                    main.startColor = MainPlugin.characterColor;
                }
                if (name == "Sparks")
                {
                    p.enabled = false;
                }
                if (name == "Glow")
                {
                    p.material = Load<Material>("RoR2/Base/Grandparent/matGrandParentSunGlow.mat");
                    p.material.SetColor("_TintColor", MainPlugin.characterColor);
                    var mat = p.material;
                    mat.mainTexture = Load<Texture2D>("RoR2/Base/Common/VFX/texFluffyCloud2Mask.png");
                    p.transform.localScale = Vector3.one * 1.5f;
                }
            }

            fireBallProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Mage/MageLightningBombProjectile.prefab"), "ShyvanaFireBallProjectile", true);
            UnityEngine.Object.Destroy(fireBallProjectile.GetComponent<AkEvent>());
            UnityEngine.Object.Destroy(fireBallProjectile.GetComponent<AkGameObj>());
            UnityEngine.Object.Destroy(fireBallProjectile.GetComponent<ProjectileProximityBeamController>());
            UnityEngine.Object.Destroy(fireBallProjectile.GetComponent<AntiGravityForce>());
            fireBallProjectile.GetComponent<ProjectileController>().ghostPrefab = fireBallProjectileGhost;
            var e = fireBallProjectile.GetComponent<ProjectileImpactExplosion>();
            e.impactEffect = fireBallProjectileExplosion;
            e.explosionEffect = fireBallProjectileExplosion;
            e.childrenCount = 1;
            e.fireChildren = true;
            e.childrenDamageCoefficient = 1;
            e.childrenProjectilePrefab = fireBallProjectileDotZone;
            ContentAddition.AddProjectile(fireBallProjectile);

            fireProjectileGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Mage/MageFireboltGhost.prefab"), "ShyvanaFireProjectileGhost", false);
            foreach (Renderer p in fireProjectileGhost.GetComponentsInChildren<Renderer>(true))
            {
                p.gameObject.SetActive(true);
                p.material.SetColor("_TintColor", MainPlugin.characterColor);
                MeshFilter mesh = p.GetComponent<MeshFilter>();
                if (mesh)
                {
                    mesh.mesh = Load<GameObject>("RoR2/Base/UtilitySkillMagazine/mdlAfterburner.fbx").GetComponentInChildren<MeshFilter>().mesh;
                }
                TrailRenderer t;
                if ((t = (p as TrailRenderer)) != null)
                {
                    t.time *= 1.5f;
                    t.widthMultiplier *= 1.5f;
                }
            }
            fireProjectileGhost.transform.localScale = Vector3.one * 1.5f;

            fireProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Mage/MageFireboltBasic.prefab"), "ShyvanaFireProjectile", true);
            fireProjectile.GetComponent<ProjectileController>().ghostPrefab = fireProjectileGhost;
            var fpE = fireProjectile.GetComponent<ProjectileImpactExplosion>();
            fpE.impactEffect = fireProjectileExplosion;
            fpE.explosionEffect = fireProjectileExplosion;
            ContentAddition.AddProjectile(fireProjectile);
        }
    }
}
