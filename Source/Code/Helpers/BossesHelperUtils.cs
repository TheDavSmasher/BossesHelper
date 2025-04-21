using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monocle;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using Celeste.Mod.BossesHelper.Code.Entities;

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

        public static bool TryCreate(this SpriteBank spriteBank, string id, out Sprite sprite)
        {
            try
            {
                sprite = spriteBank.SpriteData[id].Create();
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
        #endregion

        #region Health Displays
        private static BossesHelperSession.HealthSystemData HealthData => BossesHelperModule.Session.healthData;

        public class HealthDisplay : Entity
        {
            public readonly Sprite Frame;

            public readonly Vector2 BarScale;

            public readonly Func<int> GetHealth;

            public readonly Color BaseColor;

            public readonly int MaxHealth;

            public bool OldVisible;

            public Color Color;

            public new bool Visible
            {
                get
                {
                    return IsVisible;
                }
                set
                {
                    OldVisible = value;
                }
            }

            protected virtual bool IsVisible
            {
                get
                {
                    return base.Visible;
                }
            }

            public HealthDisplay(Vector2 position, Vector2 barScale, Func<int> getHealth, Color color = default)
            {
                Add(Frame = GFX.SpriteBank.Create(HealthData.frameSprite));
                AddTag(Tags.HUD);
                Position = position;
                BarScale = barScale;
                GetHealth = getHealth;
                MaxHealth = GetHealth();
                BaseColor = color;
                Color = color;
                OldVisible = true;
            }

            public override void Update()
            {
                base.Update();
                base.Visible = !Scene.Paused && OldVisible;
            }
        }

        public class HealthIcon : Entity
        {
            private readonly Sprite icon;

            private readonly string startAnim;

            private readonly string endAnim;

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

            public HealthIcon(Vector2 barScale, string iconSprite, string startAnim, string endAnim)
            {
                GFX.SpriteBank.TryCreate(iconSprite, out icon);
                Add(icon);
                this.startAnim = startAnim;
                this.endAnim = endAnim;
                icon.Scale = barScale;
                Tag = Tags.HUD;
            }

            public void DrawIcon(Vector2? position = null)
            {
                Position = position is Vector2 changed ? changed : Position;
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

        public class HealthIconList : HealthDisplay
        {
            private readonly bool removeIconOnDamage;

            private readonly List<HealthIcon> healthIcons = new();

            private readonly List<HealthIcon> toRemove = new();

            private readonly List<string> icons;

            private readonly List<string> createAnims;

            private readonly List<string> removeAnims;

            private readonly List<float> iconSeparations;

            private readonly bool isGlobal = false;

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
                    return healthIcons.Any(icon => icon.Visible);
                }
            }

            public HealthIconList(bool global)
                : base(HealthData.healthBarPos, HealthData.healthIconScale, () => BossesHelperModule.Session.currentPlayerHealth)
            {
                icons = SeparateList(HealthData.iconSprite);
                createAnims = SeparateList(HealthData.startAnim);
                removeAnims = SeparateList(HealthData.endAnim);
                iconSeparations = SeparateFloatList(HealthData.iconSeparation);
                removeIconOnDamage = HealthData.removeOnDamage;
                isGlobal = global;
                if (isGlobal)
                {
                    AddTag(Tags.Global);
                }
            }

            public HealthIconList(EntityData entityData, int health, Vector2 barPosition, Vector2 barScale)
                : base(barPosition, barScale, () => health)
            {
                icons = SeparateList(entityData.Attr("healthIcons"));
                createAnims = SeparateList(entityData.Attr("healthIconsCreateAnim"));
                removeAnims = SeparateList(entityData.Attr("healthIconsCreateAnim"));
                iconSeparations = SeparateFloatList(entityData.Attr("healthIconsSeparation"));
                removeIconOnDamage = entityData.Bool("removeOnDamage");
            }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                RefillHealth();
            }

            public override void Removed(Scene scene)
            {
                healthIcons.ForEach((x) => x.RemoveSelf());
                toRemove.ForEach((x) => x.RemoveSelf());
                base.Removed(scene);
            }

            public override void Update()
            {
                base.Update();
                foreach (var icon in healthIcons)
                {
                    icon.Visible = !Scene.Paused && OldVisible;
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
                        removeAnims.ElementAtOrDefault(i, removeAnims.Last())
                    );
                    if (isGlobal)
                        healthIcon.AddTag(Tags.Global);

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

            public void DecreaseHealth()
            {
                if (healthIcons.Count <= 0)
                {
                    Logger.Log("Health Render", "No Health Icon to remove");
                    return;
                }
                HealthIcon removed = healthIcons.Pop();
                removed.RemoveIcon(removeIconOnDamage);
                if (!removeIconOnDamage)
                {
                    toRemove.Add(removed);
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
            private readonly float leftEdge = GetPositionX(barPosition, barScale, barDir);

            private readonly Alignment BarDir = barDir;

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Position.X = leftEdge;
                Collider = new Hitbox(BarScale.X, BarScale.Y);
            }

            private static float GetPositionX(Vector2 position, Vector2 scale, Alignment dir)
            {
                return dir switch
                {
                    Alignment.Left => position.X - scale.X,
                    Alignment.Center => position.Y - scale.X / 2,
                    _ => position.X,
                };
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
                Position.X = BarDir switch
                {
                    Alignment.Left => leftEdge + (maxWidth - Collider.Width),
                    Alignment.Center => leftEdge + (maxWidth - Collider.Width) / 2,
                    _ => leftEdge
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
        #endregion
    }
}
