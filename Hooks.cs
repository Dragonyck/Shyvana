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
    class Hook
    {
        private static int tick = 0;
        internal static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.HasModdedDamageType(Prefabs.ruin) && damageInfo.attacker)
            {
                tick++;
                var health = damageInfo.attacker.GetComponent<HealthComponent>();
                if (health)
                {
                    health.Heal(damageInfo.damage * 0.2f, default(ProcChainMask));
                }
            }
            if (damageInfo.damageType == (DamageType.BypassBlock | DamageType.AOE) && damageInfo.attacker.GetComponent<ShyvanaBehaviour>())
            {
                damageInfo.damageType = DamageType.Generic;
                var health = damageInfo.attacker.GetComponent<HealthComponent>();
                if (health)
                {
                    health.Heal(health.fullHealth * 0.025f, default(ProcChainMask));
                }
            }
            orig(self, damageInfo);
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(Prefabs.flameBuff))
            {
                args.armorAdd += 25;
                args.moveSpeedMultAdd += 1.25f;
            }
        }
    }
}
