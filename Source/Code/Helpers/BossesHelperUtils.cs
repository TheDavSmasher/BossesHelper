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
        public static T ElementAtOrDefault<T>(this IList<T> list, int index, T @default)
        {
            return index >= 0 && index < list.Count ? list[index] : @default;
        }

        public static string String(this EntityData entityData, string key, string defaultValue = null)
        {
            if (entityData.Values != null && entityData.Values.TryGetValue(key, out var value))
            {
                if (!string.IsNullOrWhiteSpace((string) value))
                    return value.ToString();
            }
            return defaultValue;
        }

        public static bool TryPop<T>(this List<T> list, out T value, int? at = null)
        {
            int index = at is int i ? i : list.Count - 1;
            if (index < 0 || index >= list.Count)
            {
                value = default(T);
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
            return scene?.Tracker?.GetEntity<Player>();
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

        public static IEnumerator FakeDeathRoutine(this PlayerDeadBody self)
        {
            Level level = self.SceneAs<Level>();
            if (self.bounce != Vector2.Zero)
            {
                Audio.Play("event:/char/madeline/predeath", self.Position);
                self.scale = 1.5f;
                Celeste.Freeze(0.05f);
                yield return null;
                Vector2 from = self.Position;
                Vector2 to = from + self.bounce * 24f;
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.5f, start: true);
                self.Add(tween);
                tween.OnUpdate = [MethodImpl(MethodImplOptions.NoInlining)] (Tween t) =>
                {
                    self.Position = from + (to - from) * t.Eased;
                    self.scale = 1.5f - t.Eased * 0.5f;
                    self.sprite.Rotation = (float)(Math.Floor(t.Eased * 4f) * 6.2831854820251465);
                };
                yield return tween.Duration * 0.75f;
                tween.Stop();
            }
            self.Position += Vector2.UnitY * -5f;
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            self.End();
        }

        #nullable enable
        public static T Parse<T>(this string value, Func<string, IFormatProvider?, T> parser)
        {
            return parser(value, null);
        }
        #nullable disable
        #endregion

        #region Health Displays
        private static BossesHelperSession.HealthSystemData HealthData => BossesHelperModule.Session.healthData;

        public class HealthDisplay : HudEntity
        {
            public readonly Sprite Frame;

            public readonly Vector2 BarScale;

            public readonly Func<int> GetHealth;

            public readonly Color BaseColor;

            public readonly int MaxHealth;

            public bool ActiveVisibility = true;

            protected bool ActiveVisible => !Scene.Paused && ActiveVisibility;

            public Color Color;

            public new bool Visible
            {
                get
                {
                    return IsVisible;
                }
                set
                {
                    ActiveVisibility = value;
                }
            }

            protected virtual bool IsVisible
            {
                get
                {
                    return base.Visible;
                }
            }

            public HealthDisplay(Vector2 position, Vector2 barScale, Func<int> getHealth, Color color = default, bool isGlobal = false)
                : base(isGlobal)
            {
                if (GFX.SpriteBank.TryCreate(HealthData.frameSprite, out Frame))
                    Add(Frame);
                Position = position;
                BarScale = barScale;
                GetHealth = getHealth;
                MaxHealth = GetHealth();
                BaseColor = color;
                Color = color;
            }

            public override void Update()
            {
                base.Update();
                base.Visible = ActiveVisible;
            }
        }

        public class HealthIcon(Vector2 barScale, string iconSprite, string startAnim, string endAnim, bool isGlobal)
            : HudEntity(isGlobal)
        {
            private readonly Sprite icon = GFX.SpriteBank.TryCreate(iconSprite);

            public new bool Visible
            {
                get
                {
                    return icon.Visible;
                }
                set
                {
                    icon.Visible = value;
                }
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
                Add(new Coroutine(IconRoutine(startAnim)));
            }

            public void RemoveIcon(bool remove = true)
            {
                Add(new Coroutine(IconRoutine(endAnim, remove)));
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

        public class HealthIconList(Vector2 barPosition, Vector2 barScale, Func<int> getHealth, List<string> icons,
                List<string> createAnims, List<string> removeAnims, List<float> iconSeparations, bool removeIconOnDamage, bool isGlobal = false) 
            : HealthDisplay(barPosition, barScale, getHealth, isGlobal: isGlobal)
        {
            private readonly List<HealthIcon> healthIcons = [];

            private readonly List<HealthIcon> toRemove = [];

            private List<HealthIcon> AllIcons => [.. healthIcons, .. toRemove];

            public int Count
            {
                get
                {
                    return healthIcons.Count;
                }
            }

            protected override bool IsVisible
            {
                get
                {
                    return AllIcons.Any(icon => icon.Visible);
                }
            }

            public HealthIconList(bool global)
                : this(HealthData.healthBarPos, HealthData.healthIconScale, () => BossesHelperModule.Session.currentPlayerHealth,
                      SeparateList(HealthData.iconSprite), SeparateList(HealthData.startAnim), SeparateList(HealthData.endAnim), 
                      SeparateFloatList(HealthData.iconSeparation), HealthData.removeOnDamage, global) { }

            public HealthIconList(EntityData entityData, Vector2 barPosition, Vector2 barScale, Func<int> getHealth)
                : this(barPosition, barScale, getHealth, SeparateList(entityData.Attr("healthIcons")),
                      SeparateList(entityData.Attr("healthIconsCreateAnim")), SeparateList(entityData.Attr("healthIconsCreateAnim")),
                      SeparateFloatList(entityData.Attr("healthIconsSeparation")), entityData.Bool("removeOnDamage")) { }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                RefillHealth();
            }

            public override void Removed(Scene scene)
            {
                AllIcons.ForEach((x) => x.RemoveSelf());
                base.Removed(scene);
            }

            public override void Render()
            {
                base.Render();
                foreach (var icon in AllIcons)
                {
                    icon.Visible = ActiveVisible;
                }
            }

            public void RefillHealth(int? upTo = null)
            {
                int limit = upTo is int to ? healthIcons.Count + to : MaxHealth;
                for (int i = healthIcons.Count; i < limit; i++)
                {
                    IncreaseHealth(i);
                }
            }

            public void IncreaseHealth(int i)
            {
                Vector2? iconPosition = null;
                if (!toRemove.TryPop(out HealthIcon healthIcon))
                {
                    healthIcon = new(BarScale,
                        icons.ElementAtOrDefault(i, icons.Last()),
                        createAnims.ElementAtOrDefault(i, createAnims.Last()),
                        removeAnims.ElementAtOrDefault(i, removeAnims.Last()),
                        IsGlobal
                    );

                    float sum = 0f;
                    for (int index = 0; index < i; index++)
                    {
                        sum += iconSeparations.ElementAtOrDefault(index, iconSeparations.LastOrDefault());
                    }
                    iconPosition = Position + Vector2.UnitX * sum;
                }
                healthIcons.Add(healthIcon);
                Scene.Add(healthIcon);
                healthIcon.DrawIcon(iconPosition);
            }

            public void DecreaseHealth(int amount = 1)
            {
                if (healthIcons.Count <= 0)
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
            List<string> res = listString.Split(',').ToList();
            for (int i = 0; i < res.Count; i++)
            {
                res[i] = res[i].Trim();
            }
            return res;
        }

        public static string JoinList(string[] list)
        {
            return string.Join(",", list);
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

        public abstract class GlobalEntity : Entity
        {
            protected readonly bool IsGlobal;

            public GlobalEntity(bool isGlobal)
            {
                if (IsGlobal = isGlobal)
                {
                    AddTag(Tags.Global);
                }
            }
        }
        #endregion
    }
}
