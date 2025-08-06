using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monocle;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    public static class BossesHelperUtils
    {
        #region Extensions
        public static T ElementAtOrLast<T>(this IList<T> list, int index)
        {
            return index >= 0 && index < list.Count ? list[index] : list.LastOrDefault();
        }

        public static bool TryPop<T>(this List<T> list, out T value, int? at = null)
        {
            int index = at is int i ? i : list.Count - 1;
            if (index < 0 || index >= list.Count)
            {
                value = default;
                return false;
            }
            value = list.Pop(index);
            return true;
        }

        public static T Pop<T>(this List<T> list, int? at = null)
        {
            int index = at is int i ? i : list.Count - 1;
            T popped = list.ElementAt(index);
            list.RemoveAt(index);
            return popped;
        }

        public static Player GetPlayer(this Scene scene)
        {
            return scene.GetEntity<Player>();
        }

        public static T GetEntity<T>(this Scene scene) where T : Entity => scene?.Tracker.GetEntity<T>();

        public static IEnumerator PlayAnim(this Sprite sprite, string anim)
        {
            bool singleLoop = false;
            void waitUntilDone(string _) => singleLoop = true;
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

        public static bool TryPlay(this Sprite sprite, string anim)
        {
            if (!sprite.Has(anim)) return false;
            sprite.Play(anim);
            return true;
        }

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

        public static T Parse<T>(this string value) where T : IParsable<T>
        {
            return T.Parse(value, null);
        }

        public static Vector2 NearestWhole(this Vector2 value)
        {
            return new Vector2((int) value.X, (int) value.Y);
        }

        public static void Coroutine(this IEnumerator enumerator, Entity target)
        {
            target.Add(new Coroutine(enumerator));
        }
        #endregion

        #region Health Displays
        private static BossesHelperSession.HealthSystemData HealthData => BossesHelperModule.Session.healthData;

        public class HealthDisplay(Vector2 position, Vector2 barScale, Func<int> getHealth, Color color = default, bool isGlobal = false)
            : HudEntity(isGlobal)
        {
            public readonly Sprite Frame = GFX.SpriteBank.TryCreate(HealthData.frameSprite);

            public readonly Vector2 BarScale = barScale;

            public readonly Func<int> GetHealth = getHealth;

            public readonly Color BaseColor = color;

            public readonly int MaxHealth = getHealth();

            private bool ActiveVisibility = true;

            protected bool ActiveVisible => !Scene.Paused && ActiveVisibility;

            public Color Color = color;

            public new bool Visible
            {
                get => IsVisible;
                set => ActiveVisibility = value;
            }

            protected virtual bool IsVisible => base.Visible;

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Position = position;
                if (Frame.Width > 0 && Frame.Height > 0)
                {
                    Add(Frame);
                }
            }

            public override void Update()
            {
                base.Update();
                base.Visible = ActiveVisible;
            }
        }

        public class HealthIconList(Vector2 barPosition, Vector2 barScale, Func<int> getHealth, bool isGlobal, List<string> icons,
                List<string> createAnims, List<string> removeAnims, List<float> iconSeparations, bool removeIconOnDamage) 
            : HealthDisplay(barPosition, barScale, getHealth, isGlobal: isGlobal)
        {
            private class HealthIcon(Vector2 barScale, bool isGlobal, string iconSprite, string startAnim, string endAnim)
            : HudEntity(isGlobal)
            {
                private readonly Sprite icon = GFX.SpriteBank.TryCreate(iconSprite);

                public new bool Visible
                {
                    get => icon.Visible;
                    set => icon.Visible = value;
                }

                public override void Added(Scene scene)
                {
                    base.Added(scene);
                    if (icon.Width > 0 && icon.Height > 0)
                    {
                        icon.Scale = barScale;
                        Add(icon);
                    }
                }

                public void DrawIcon(Vector2? position = null)
                {
                    Position = position ?? Position;
                    IconRoutine(startAnim).Coroutine(this);
                }

                public void RemoveIcon(bool remove = true)
                {
                    IconRoutine(endAnim, remove).Coroutine(this);
                }

                private IEnumerator IconRoutine(string anim, bool remove = false)
                {
                    yield return icon.PlayAnim(anim);
                    if (remove)
                    {
                        RemoveSelf();
                    }
                }
            }

            private readonly List<HealthIcon> healthIcons = [];

            private readonly List<HealthIcon> toRemove = [];

            private List<HealthIcon> AllIcons => [.. healthIcons, .. toRemove];

            public int Count => healthIcons.Count;

            protected override bool IsVisible => AllIcons.Any(icon => icon.Visible);

            public HealthIconList(bool global = false)
                : this(HealthData.healthBarPos, HealthData.healthIconScale, () => BossesHelperModule.Session.currentPlayerHealth,
                      global, SeparateList(HealthData.iconSprite), SeparateList(HealthData.startAnim),
                      SeparateList(HealthData.endAnim), SeparateFloatList(HealthData.iconSeparation), HealthData.removeOnDamage) { }

            public HealthIconList(EntityData entityData, Vector2 barPosition, Vector2 barScale, Func<int> getHealth)
                : this(barPosition, barScale, getHealth, false, SeparateList(entityData.Attr("healthIcons")),
                      SeparateList(entityData.Attr("healthIconsCreateAnim")), SeparateList(entityData.Attr("healthIconsCreateAnim")),
                      SeparateFloatList(entityData.Attr("healthIconsSeparation")), entityData.Bool("removeOnDamage")) { }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                RefillHealth();
            }

            public override void Removed(Scene scene)
            {
                AllIcons.ForEach(x => x.RemoveSelf());
                base.Removed(scene);
            }

            public override void Render()
            {
                base.Render();
                AllIcons.ForEach(icon => icon.Visible = ActiveVisible);
            }

            public void RefillHealth(int? upTo = null)
            {
                int limit = upTo + Count ?? MaxHealth;
                for (int i = Count; i < limit; i++)
                {
                    IncreaseHealth(i);
                }
            }

            public void IncreaseHealth(int i)
            {
                Vector2? iconPosition = null;
                if (!toRemove.TryPop(out HealthIcon healthIcon))
                {
                    healthIcon = new(BarScale, IsGlobal, icons.ElementAtOrLast(i),
                        createAnims.ElementAtOrLast(i), removeAnims.ElementAtOrLast(i));

                    float sum = 0f;
                    for (int index = 0; index < i; index++)
                    {
                        sum += iconSeparations.ElementAtOrLast(index);
                    }
                    iconPosition = Position + Vector2.UnitX * sum;
                }
                healthIcons.Add(healthIcon);
                Scene.Add(healthIcon);
                healthIcon.DrawIcon(iconPosition);
            }

            public void DecreaseHealth(int amount = 1)
            {
                if (Count <= 0)
                {
                    Logger.Log("Health Render", "No Health Icon to remove");
                    return;
                }
                for (int i = 0; i < amount; i++)
                {
                    HealthIcon removed = healthIcons.Pop();
                    removed.RemoveIcon(removeIconOnDamage);
                    if (!removeIconOnDamage)
                    {
                        toRemove.Add(removed);
                    }
                }
            }

            public void Clear()
            {
                healthIcons.Clear();
                toRemove.Clear();
            }
        }

        public class HealthNumber(Vector2 barPosition, Vector2 barScale, Func<int> bossHealth, Color color)
            : HealthDisplay(barPosition, barScale, bossHealth, color)
        {
            public override void Update()
            {
                base.Update();
                if (Color != BaseColor)
                {
                    Color = Color.Lerp(Color, BaseColor, 0.1f);
                }
            }

            public override void Render()
            {
                ActiveFont.Draw($"{GetHealth()}", Position, new Vector2(0.5f, 0.5f), BarScale, Color);
                base.Render();
            }
        }

        public class HealthBar(Vector2 barPosition, Vector2 barScale, Func<int> bossHealth, Color color, Alignment barDir)
            : HealthDisplay(barPosition, barScale, bossHealth, color)
        {
            private readonly float leftEdge = barDir switch
            {
                Alignment.Left => barPosition.X - barScale.X,
                Alignment.Center => barPosition.Y - barScale.X / 2,
                _ => barPosition.X,
            };

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Position.X = leftEdge;
                Collider = new Hitbox(BarScale.X, BarScale.Y);
            }

            public override void Update()
            {
                base.Update();
                if (Color != BaseColor)
                {
                    Color = Color.Lerp(Color, BaseColor, 0.1f);
                }
                float maxWidth = BarScale.X;
                Collider.Width = maxWidth * GetHealth() / MaxHealth;
                Position.X = leftEdge + barDir switch
                {
                    Alignment.Left => (maxWidth - Collider.Width),
                    Alignment.Center => (maxWidth - Collider.Width) / 2,
                    _ => 0
                };
            }

            public override void Render()
            {
                Draw.Rect(Collider, Color);
                base.Render();
            }
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
            return SeparateList(listString).Select(float.Parse).ToList();
        }

        public class Crc32
        {
            private const UInt32 s_generator = 0xEDB88320;

            public Crc32()
            {
                m_checksumTable = Enumerable.Range(0, 256).Select(i =>
                {
                    var tableEntry = (uint)i;
                    for (var j = 0; j < 8; ++j)
                    {
                        tableEntry = ((tableEntry & 1) != 0)
                            ? (s_generator ^ (tableEntry >> 1))
                            : (tableEntry >> 1);
                    }
                    return tableEntry;
                }).ToArray();
            }

            public int Get(string value)
            {
                IEnumerable<char> byteStream = value.ToCharArray();
                return (int)~byteStream.Aggregate(0xFFFFFFFF, (checksumRegister, currentByte) =>
                            (m_checksumTable[(checksumRegister & 0xFF) ^ Convert.ToByte(currentByte)] ^ (checksumRegister >> 8)));
            }

            private readonly UInt32[] m_checksumTable;
        }

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
        #endregion
    }
}
