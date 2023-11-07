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
    class BezierCurveLineScaler : BezierCurveLine
    {
        private int size = 98;
        private void FixedUpdate()
        {
            size = Mathf.RoundToInt(Util.Remap(Vector3.Distance(base.transform.position, endTransform.position), 0, 280, 24, 98));
            if (lineRenderer.numPositions != size)
            {
                lineRenderer.numPositions = size;
                Array.Resize<Vector3>(ref vertexList, size + 1);
            }
        }
    }
}
