using Harmony;
using System.Reflection;
using System;
using UnityEngine;
using AdvancedRoadTools.NewData;
using AdvancedRoadTools.NewManager;

namespace AdvancedRoadTools.Util
{
    public static class HarmonyDetours
    {
        private static HarmonyInstance harmony = null;
        private static void ConditionalPatch(this HarmonyInstance harmony, MethodBase method, HarmonyMethod prefix, HarmonyMethod postfix)
        {
            var fullMethodName = string.Format("{0}.{1}", method.ReflectedType?.Name ?? "(null)", method.Name);
            if (HarmonyInstance.GetPatchInfo(method)?.Owners?.Contains(harmony.Id) == true)
            {
                DebugLog.LogToFileOnly("Harmony patches already present for {0}" + fullMethodName.ToString());
            }
            else
            {
                DebugLog.LogToFileOnly("Patching {0}..." + fullMethodName.ToString());
                harmony.Patch(method, prefix, postfix);
            }
        }

        private static void ConditionalUnPatch(this HarmonyInstance harmony, MethodBase method, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
        {
            var fullMethodName = string.Format("{0}.{1}", method.ReflectedType?.Name ?? "(null)", method.Name);
            if (prefix != null)
            {
                DebugLog.LogToFileOnly("UnPatching Prefix{0}..." + fullMethodName.ToString());
                harmony.Unpatch(method, HarmonyPatchType.Prefix);
            }
            if (postfix != null)
            {
                DebugLog.LogToFileOnly("UnPatching Postfix{0}..." + fullMethodName.ToString());
                harmony.Unpatch(method, HarmonyPatchType.Postfix);
            }
        }

        public static void Apply()
        {
            harmony = HarmonyInstance.Create("AdvancedRoadTools");
            var netManagerReleaseNodeImplementation = typeof(NetManager).GetMethod("ReleaseNodeImplementation", BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder, new Type[2]{typeof(ushort),typeof(NetNode).MakeByRefType()}, null);
            var netManagerReleaseNodeImplementationPreFix = typeof(CustomNetManager).GetMethod("NetManagerReleaseNodeImplementationPreFix");
            harmony.ConditionalPatch(netManagerReleaseNodeImplementation,
                new HarmonyMethod(netManagerReleaseNodeImplementationPreFix),
                null);
            Loader.HarmonyDetourFailed = false;
            DebugLog.LogToFileOnly("Harmony patches applied");
        }

        public static void DeApply()
        {
            //1
            var netManagerReleaseNodeImplementation = typeof(NetManager).GetMethod("ReleaseNodeImplementation", BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder, new Type[2] { typeof(ushort), typeof(NetNode).MakeByRefType() }, null);
            var netManagerReleaseNodeImplementationPreFix = typeof(CustomNetManager).GetMethod("NetManagerReleaseNodeImplementationPreFix");
            harmony.ConditionalUnPatch(netManagerReleaseNodeImplementation,
                new HarmonyMethod(netManagerReleaseNodeImplementationPreFix),
                null);
            DebugLog.LogToFileOnly("Harmony patches DeApplied");
        }
    }
}
