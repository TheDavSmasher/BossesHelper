using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.BossesHelper.Code.Helpers;

namespace Celeste.Mod.BossesHelper
{
    public partial class BossesHelperModule
    {
        public static void ILOnSquish(ILContext il)
        {
            ILCursor dieCursor = new(il);
            while (dieCursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("Die")))
            {
                ILCursor argCursor = new(dieCursor);
                if (argCursor.TryGotoPrev(MoveType.AfterLabel, instr => instr.MatchLdarg0()))
                {
                    //KillOnCrush(self, data, evenIfInvincible);
                    argCursor.EmitLdarg0()
                        .EmitLdarg1()
                        .EmitLdloc2()
                        .EmitDelegate(KillOnCrush);
                }
            }
        }
    }

    namespace Code
    {
        namespace Helpers
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

        namespace Entities
        {
            public partial class HealthSystemManager
            {
                private static partial void LoadFakeDeathHooks()
                {
                    foreach (string fakeMethod in HealthData.FakeDeathMethods)
                    {
                        string[] opts = fakeMethod.Split(':');
                        if (opts.Length != 2)
                            continue;
                        if (LuaMethodWrappers.GetTypeFromString(opts[0], "")?
                            .GetMethod(opts[1], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            is MethodInfo methodInfo)
                        {
                            ILHookHelper.GenerateHookOn(fakeMethod, methodInfo, il =>
                            {
                                ILCursor cursor = new(il);
                                cursor.EmitDelegate(BossesHelperExports.UseFakeDeath);
                                while (cursor.TryGotoNext(instr => instr.MatchRet()))
                                {
                                    cursor.EmitDelegate(BossesHelperExports.ClearFakeDeath);
                                    cursor.Index++;
                                }
                            });
                        }
                    }
                }

                private static partial void UnloadFakeDeathHooks()
                {
                    foreach (string fakeMethod in HealthData.FakeDeathMethods)
                    {
                        ILHookHelper.DisposeHook(fakeMethod);
                    }
                }
            }
        }

        namespace Components
        {
            public partial class GlobalSavePointChanger : Component
            {
                public void AddToEntityOnMethod<T>(T entity, string method,
                    BindingFlags flags = BindingFlags.Default, bool stateMethod = false) where T : Entity
                {
                    entity.Add(this);
                    ILHookHelper.GenerateHookOn(typeof(T), method, AddUpdateDelegate, flags, stateMethod);
                }

                private static void AddUpdateDelegate(ILContext il)
                {
                    //this.Get<GlobalSavePointChanger>()?.Update();
                    new ILCursor(il)
                        .EmitLdarg0()
                        .EmitDelegate(UpdateSavePointChanger);
                }

                private static void UpdateSavePointChanger(Entity entity)
                {
                    entity.Get<GlobalSavePointChanger>()?.Update();
                }
            }
        }
    }
}