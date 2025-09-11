using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
	public static class BossesHelperUtils
	{
		#region Extensions
		#region Scene
		public static Player GetPlayer(this Scene scene)
		{
			return scene.GetEntity<Player>();
		}

		public static T GetEntity<T>(this Scene scene) where T : Entity => scene?.Tracker.GetEntity<T>();

		public static void DoNotLoad(this Scene scene, EntityID entityID)
		{
			(scene as Level).Session.DoNotLoad.Add(entityID);
		}
		#endregion

		#region Sprite
		public static IEnumerator PlayAnim(this Sprite sprite, string anim)
		{
			bool singleLoop = false;
			void waitUntilDone(string _) => singleLoop = true;
			if (sprite is null) yield break;
			sprite.OnLastFrame += waitUntilDone;
			if (sprite.TryPlay(anim))
			{
				while (!singleLoop && sprite.Animating)
				{
					yield return null;
				}
			}
			sprite.OnLastFrame -= waitUntilDone;
		}

		public static void PlayOrWarn(this Sprite sprite, string anim)
		{
			if (!sprite.TryPlay(anim))
			{
				Logger.Warn("BossesHelper", "Animation specified does not exist!");
			}
		}

		public static bool TryPlay(this Sprite sprite, string anim)
		{
			if (!sprite.Has(anim)) return false;
			sprite.Play(anim);
			return true;
		}
		#endregion

		#region SpriteBank
		public static Sprite TryCreate(this SpriteBank spriteBank, string id)
		{
			try
			{
				return spriteBank.Create(id);
			}
			catch (Exception)
			{
				return new Sprite();
			}
		}

		public static bool TryCreate(this SpriteBank spriteBank, string id, out Sprite sprite)
		{
			try
			{
				sprite = spriteBank.Create(id);
				return true;
			}
			catch (Exception)
			{
				sprite = new Sprite();
				return false;
			}
		}
		#endregion

		#region string
		public static T Parse<T>(this string value) where T : IParsable<T>
		{
			return T.Parse(value, null);
		}

		public static bool TryParse<T>(this string value, out T result) where T : IParsable<T>
		{
			return T.TryParse(value, null, out result);
		}

		public enum SplitMode
		{
			Exclude,
			IncludeFirst,
			IncludeLast
		}

		public static (string, string) SplitOnce(this string val, char character, bool forward = true, SplitMode mode = SplitMode.Exclude, string preDefault = null)
		{
			int index = forward ? val.IndexOf(character) : val.LastIndexOf(character);
			if (index == -1)
				return (preDefault, val);
			if (mode == SplitMode.IncludeFirst)
				index++;
			string first = val[..index];
			if (mode == SplitMode.Exclude)
				index++;
			string second = val[index..];
			return (first, second);
		}
		#endregion

		public static T ElementAtOrLast<T>(this IList<T> list, int index)
		{
			return index >= 0 && index < list.Count ? list[index] : list.LastOrDefault();
		}

		public static Vector2 NearestWhole(this Vector2 value)
		{
			return new Vector2((int)value.X, (int)value.Y);
		}

		public static void Coroutine(this IEnumerator enumerator, Entity target)
		{
			new Coroutine(enumerator).AddTo(target);
		}

		public static void AddTo(this Component self, Entity target)
		{
			target.Add(self);
		}

		public static void PositionTween(this Entity self, Vector2 target, float time, Ease.Easer easer = null)
		{
			Tween.Position(self, target, time, easer);
		}
		#endregion

		#region Helpers
		public enum Alignment
		{
			Left = -1,
			Center,
			Right
		}

		public static float DistanceBetween(Vector2 start, Vector2 end)
		{
			var dx = start.X - end.X;
			var dy = start.Y - end.Y;
			return (float)Math.Sqrt(dx * dx + dy * dy);
		}

		public static List<string> SeparateList(string listString)
		{
			List<string> res = listString?.Split(',').ToList() ?? [];
			for (int i = 0; i < res.Count; i++)
			{
				res[i] = res[i].Trim();
			}
			return res;
		}

		public static List<float> SeparateFloatList(string listString)
		{
			return [.. SeparateList(listString).Select(float.Parse)];
		}

		public class EnumDict<TKey, TValue> : Dictionary<TKey, TValue> where TKey : struct, Enum
		{
			public EnumDict(Func<TKey, TValue> populator) : base()
			{
				foreach (var option in Enum.GetValues<TKey>())
				{
					Add(option, populator(option));
				}
			}
		}

		public class Crc32
		{
			private const UInt32 s_generator = 0xEDB88320;

			public Crc32()
			{
				m_checksumTable = [.. Enumerable.Range(0, 256).Select(i =>
				{
					var tableEntry = (uint)i;
					for (var j = 0; j < 8; ++j)
					{
						tableEntry = ((tableEntry & 1) != 0)
							? (s_generator ^ (tableEntry >> 1))
							: (tableEntry >> 1);
					}
					return tableEntry;
				})];
			}

			public int Get(string value)
			{
				IEnumerable<char> byteStream = value.ToCharArray();
				return (int)~byteStream.Aggregate(0xFFFFFFFF, (checksumRegister, currentByte) =>
							(m_checksumTable[(checksumRegister & 0xFF) ^ Convert.ToByte(currentByte)] ^ (checksumRegister >> 8)));
			}

			private readonly UInt32[] m_checksumTable;
		}
		#endregion

		#region Celeste Classes
		public abstract class HudEntity : GlobalEntity
		{
			public HudEntity(bool isGlobal = false) : base(isGlobal)
			{
				AddTag(Tags.HUD);
			}
		}

		public abstract class GlobalEntity(bool isGlobal) : Entity
		{
			protected bool IsGlobal { get; private set; } = isGlobal;

			public override void Added(Scene scene)
			{
				base.Added(scene);
				if (IsGlobal)
					ChangeGlobalState(true);
			}

			public void ChangeGlobalState(bool state)
			{
				if (IsGlobal != state)
				{
					IsGlobal = state;
					if (state)
						AddTag(Tags.Global);
					else
						RemoveTag(Tags.Global);
				}
			}
		}

		public class SingleUse<T> where T : struct
		{
			public T? Value
			{
				get
				{
					T? value = field;
					field = null;
					return value;
				}
				set;
			}
		}

		public class Stopwatch(float fullTime) : Component(true, false)
		{
			public float TimeLeft { get; set; }

			public bool Finished => TimeLeft <= 0;

			public void Reset()
			{
				TimeLeft = fullTime;
			}

			public override void Update()
			{
				if (TimeLeft > 0)
					TimeLeft -= Engine.DeltaTime;
			}
		}

		public static void AddIFramesWatch(this Player player, bool onlyOnNull = false)
		{
			if (player.Get<Stopwatch>() is Stopwatch watch)
			{
				if (onlyOnNull)
					return;
				watch.RemoveSelf();
			}
			player.Add(new Stopwatch(BossesHelperModule.Session.healthData.damageCooldown));
		}

		public interface IMonocleCollection<T> : IReadOnlyCollection<T>
		{
			public bool Active
			{
				get => this.Any(i => ItemActive(i));
				set
				{
					foreach (var item in this)
						ItemActive(item) = value;
				}
			}

			public bool Visible
			{
				get => this.Any(i => ItemVisible(i));
				set
				{
					foreach (var item in this)
						ItemVisible(item) = value;
				}
			}

			ref bool ItemActive(T item);
			ref bool ItemVisible(T item);
		}

		public class ComponentStack<T> : Stack<T>, IMonocleCollection<T> where T : Component
		{
			public ref bool ItemActive(T item) => ref item.Active;

			public ref bool ItemVisible(T item) => ref item.Visible;
		}
		#endregion
	}
}
