using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    public static class ILHookHelper
    {
        private static readonly Dictionary<string, ILHook> createdILHooks = new();

        public static void GenerateHookOn(Type classType, string method,
            ILContext.Manipulator action, BindingFlags flags = BindingFlags.Default, bool stateMethod = false)
        {
            string key = classType.Name + ":" + method;
            if (classType.GetMethod(method, flags) is not MethodInfo methodInfo) return;
            if (stateMethod)
            {
                methodInfo = methodInfo.GetStateMachineTarget();
            }
            GenerateHookOn(key, methodInfo, action);
        }

        public static void GenerateHookOn(MethodInfo methodInfo, ILContext.Manipulator action)
        {
            string key = methodInfo.DeclaringType.Name + ":" + methodInfo.Name;
            GenerateHookOn(key, methodInfo, action);
        }

        public static void GenerateHookOn(string key, MethodInfo methodInfo, ILContext.Manipulator action)
        {
            if (createdILHooks.ContainsKey(key)) return;
            ILHook newHook = new(methodInfo, action);
            createdILHooks.Add(key, newHook);
            newHook.Apply();
        }

        public static void DisposeHook(Type classType, string method)
        {
            string key = classType.Name + ":" + method;
            DisposeHook(key);
        }

        public static void DisposeHook(string key)
        {
            if (createdILHooks.TryGetValue(key, out ILHook toRemove))
            {
                toRemove.Dispose();
                createdILHooks.Remove(key);
            }
        }

        public static void DisposeAll()
        {
            foreach (ILHook hook in createdILHooks.Values)
            {
                hook.Dispose();
            }
            createdILHooks.Clear();
        }
    }
}
