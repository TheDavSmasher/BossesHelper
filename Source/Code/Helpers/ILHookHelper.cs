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
            if (createdILHooks.ContainsKey(key)) return;
            MethodInfo methodInfo = classType.GetMethod(method, flags);
            if (stateMethod)
            {
                methodInfo = methodInfo.GetStateMachineTarget();
            }
            createdILHooks.Add(key, new ILHook(methodInfo, action));
        }

        public static void DisposeHook(Type classType, string method)
        {
            string key = classType.Name + ":" + method;
            createdILHooks[key].Dispose();
            createdILHooks.Remove(key);
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
