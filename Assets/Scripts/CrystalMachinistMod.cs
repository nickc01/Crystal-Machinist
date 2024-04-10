using Modding;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class CrystalMachinistMod : WeaverMod
{
    public CrystalMachinistMod() : base("Crystal Machinist") { }

    public override string GetVersion()
    {
        return "2.0.2.0";
    }

    public override void Initialize()
    {
        //var logLevelField = typeof(Modding.Logger).GetField("_logLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        //logLevelField.SetValue(null, LogLevel.Fine);
        ModHooks.LanguageGetHook += GodhomeLanguageHook;

        /*foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var portalManager = assembly.GetType("AnyRadiance.Radiance.PortalManager");

            if (portalManager != null)
            {
                var harmonyPatcher = HarmonyPatcher.Create("com.crystalMachinst.test");

                var sourceMethod1 = portalManager.GetMethod("OnEnterPortal1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var sourceMethod2 = portalManager.GetMethod("OnEnterPortal2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var printPrefix = typeof(CrystalMachinistMod).GetMethod(nameof(PrintRootPrefix), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

                harmonyPatcher.Patch(sourceMethod1, printPrefix, null);
                harmonyPatcher.Patch(sourceMethod2, printPrefix, null);

                break;
            }
        }*/
    }

    /*static List<GameObject> doNotTeleport;

    static bool PrintRootPrefix(object __instance, object portal, GameObject teleported)
    {
        if (__instance.GetType().Name.Contains("PortalManager"))
        {
            if (doNotTeleport == null)
            {
                doNotTeleport = __instance.ReflectGetField("_doNotTeleport") as List<GameObject>;
            }

            if (!doNotTeleport.Contains(teleported))
            {
                int hiearchy = 0;

                Transform parent = teleported.transform;

                var isEnclosed = portal.ReflectCallMethod("IsEnclosing",new object[] {teleported});

                WeaverLog.Log($"{teleported.name} enclosed in {((Component)portal).name} = {isEnclosed}");

                var otherBounds = teleported.GetComponent<Collider2D>().bounds;

                var _renderer = (MeshRenderer)portal.ReflectGetField("_renderer");

                var portalBounds = _renderer.bounds;

                WeaverLog.Log($"{nameof(otherBounds)}: MinX = {otherBounds.min.x}, MinY = {otherBounds.min.y}, MaxX = {otherBounds.max.x}, MaxY = {otherBounds.max.y}");
                WeaverLog.Log($"{nameof(portalBounds)}: MinX = {portalBounds.min.x}, MinY = {portalBounds.min.y}, MaxX = {portalBounds.max.x}, MaxY = {portalBounds.max.y}");

                WeaverLog.Log($"ROOT of {teleported.name} = {teleported.transform.root.name}");
                while (true)
                {
                    WeaverLog.Log($"Teleporting Object Hiearchy {hiearchy} = {parent.name}");
                    if (parent.parent != null)
                    {
                        parent = parent.parent;
                        hiearchy++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return true;
    }*/

    internal static string GodhomeLanguageHook(string key, string sheetTitle, string res)
    {
        if (key == "NAME_MEGA_BEAM_MINER_2")
        {
            return "Crystal Machinist";
        }
        else
        {
            return res;
        }
    }
}
