﻿using Celeste.Mod.BossesHelper.Code.Helpers;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

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

		namespace Helpers
		{
			public static class ILHookHelper
			{
				private static readonly Dictionary<string, ILHook> createdILHooks = [];

				public static MethodInfo GetMethodInfo(Type type, string method,
					BindingFlags flags = BindingFlags.Default, bool stateMethod = false)
				{
					return type.GetMethod(method, flags) is not MethodInfo methodInfo ? null :
						stateMethod ? methodInfo.GetStateMachineTarget() : methodInfo;
				}

				private static string GetKey(MethodInfo methodInfo) => GetKey(methodInfo.DeclaringType, methodInfo.Name);

				private static string GetKey(Type type, string method) => $"{type.Name}:{method}";

				public static void GenerateHookOn(Type classType, string method,
					ILContext.Manipulator action, BindingFlags flags = BindingFlags.Default, bool stateMethod = false)
					=> GenerateHookOn(GetKey(classType, method), GetMethodInfo(classType, method, flags, stateMethod), action);

				public static void GenerateHookOn(MethodInfo methodInfo, ILContext.Manipulator action)
					=> GenerateHookOn(GetKey(methodInfo), methodInfo, action);

				public static void GenerateHookOn(string key, MethodInfo methodInfo, ILContext.Manipulator action)
				{
					createdILHooks.TryAdd(key, new(methodInfo, action));
				}

				public static void DisposeHook(MethodInfo methodInfo)
					=> DisposeHook(GetKey(methodInfo));

				public static void DisposeHook(Type classType, string method)
					=> DisposeHook(GetKey(classType, method));

				public static void DisposeHook(string key)
				{
					if (createdILHooks.Remove(key, out ILHook hook))
						hook.Dispose();
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
				public static partial void LoadFakeDeathHooks()
				{
					foreach (string fakeMethod in HealthData.FakeDeathMethods)
					{
						var (classType, methodName) = fakeMethod.SplitOnce(':');
						if (classType == null)
							continue;

						var (classPrefix, className) = classType.SplitOnce('.', false, SplitMode.IncludeFirst, "Celeste.");

						if (LuaMethodWrappers.GetTypeFromString(className, classPrefix)?
							.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
							is MethodInfo methodInfo)
						{
							ILHookHelper.GenerateHookOn(fakeMethod, methodInfo, il =>
							{
								ILCursor cursor = new(il);
								cursor.EmitDelegate(FakeDeathHook);
								while (cursor.TryGotoNext(instr => instr.MatchRet()))
								{
									cursor.EmitDelegate(BossesHelperExports.ClearFakeDeath);
									cursor.Index++;
								}
							});
						}
					}
				}

				private static void FakeDeathHook()
				{
					if (IsEnabled)
					{
						ModSession.useFakeDeath = true;
					}
				}

				public static partial void UnloadFakeDeathHooks()
				{
					foreach (string fakeMethod in HealthData.FakeDeathMethods)
					{
						ILHookHelper.DisposeHook(fakeMethod);
					}
				}
			}
		}
	}
}