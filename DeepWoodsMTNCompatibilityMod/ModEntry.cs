using Harmony;
using StardewModdingAPI;
using System;
using System.Reflection;

namespace DeepWoodsMTNCompatibilityMod
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            if (Helper.ModRegistry.IsLoaded("maxvollmer.deepwoodsmod")
                && Helper.ModRegistry.IsLoaded("SgtPickles.MTN"))
            {
                PatchMTN();
            }
            else
            {
                Monitor.Log("Either MTN or DeepWoods or both are not installed. This mod won't do anything. You can remove this mod or safely ignore this warning.", LogLevel.Warn);
            }
        }

        private Assembly GetAssembly(string name)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                if (a.GetName().Name == name)
                    return a;

            throw new Exception("Assembly " + name + "does not exist.");
        }

        private Type GetType(string assemblyName, string typeName)
        {
            foreach (Type t in GetAssembly(assemblyName).GetTypes())
            {
                if (t.Name == typeName)
                    return t;
            }

            throw new Exception("Type " + typeName + " does not exist in assembly " + assemblyName + ".");
        }

        private void PatchMTN()
        {
            Type mtnMultiplerType = GetType("MTN", "MTNMultiplayer");
            MethodInfo mtnMethod = mtnMultiplerType.GetMethod("processIncomingMessage", BindingFlags.Instance | BindingFlags.Public);
            if (mtnMethod == null)
            {
                Monitor.Log("Couldn't patch MTN for DeepWoods compatibility. Network method required for patching not found in MTN. Check if there is an update for this patch mod.", LogLevel.Error);
                return;
            }

            Type deepwoodsMultiplerType = GetType("DeepWoodsMod", "InterceptingMultiplayer");
            MethodInfo deepwoodsMethod = deepwoodsMultiplerType.GetMethod("InternalProcessIncomingMessage", BindingFlags.Static | BindingFlags.NonPublic);
            if (deepwoodsMethod == null)
            {
                Monitor.Log("Couldn't patch MTN for DeepWoods compatibility. Network method required for patching not found in DeepWoodsMod. Make sure you have the latest version of DeepWoods installed or check if there is an update for this patch mod.", LogLevel.Error);
                return;
            }

            HarmonyInstance harmonyInstance = HarmonyInstance.Create("maxvollmer.deepwoodsmtncompatibilitymod");
            harmonyInstance.Patch(mtnMethod, new HarmonyMethod(deepwoodsMethod));
        }
    }
}
