using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    public static class HealthBarUtils
    {
        public static T ElementAtOrDefault<T>(this IList<T> list, int index, T @default)
        {
            return index >= 0 && index < list.Count ? list[index] : @default;
        }

        private static BossesHelperSession.HealthSystemData HealthData => BossesHelperModule.Session.healthData;

        public class HealthIcon : Entity
        {
            private readonly Sprite icon;

            private readonly string startAnim;

            private readonly string endAnim;

            public HealthIcon(Vector2 barScale, string iconSprite, string startAnim, string endAnim)
            {
                Add(icon = GFX.SpriteBank.Create(iconSprite));
                this.startAnim = startAnim;
                this.endAnim = endAnim;
                icon.Scale = barScale;
                Tag = Tags.HUD;
            }

            public void DrawIcon(Vector2 position)
            {
                Position = position;
                Add(new Coroutine(DrawRoutine()));
            }

            private IEnumerator DrawRoutine()
            {
                if (!string.IsNullOrEmpty(startAnim) && icon.Has(startAnim))
                    icon.Play(startAnim);
                yield return 0.32f;
            }

            public void RemoveIcon()
            {
                Add(new Coroutine(RemoveRoutine()));
            }

            private IEnumerator RemoveRoutine()
            {
                if (!string.IsNullOrEmpty(endAnim) && icon.Has(endAnim))
                    icon.Play(endAnim);
                yield return 0.88f;
                RemoveSelf();
            }

            public override void Render()
            {
                base.Render();
                icon.Visible = !Scene.Paused;
            }
        }

        public class HealthIconList : Entity
        {
            private readonly bool useSessionValues;

            private readonly List<HealthIcon> healthIcons;

            private List<string> icons;

            private List<string> createAnims;

            private List<string> removeAnims;

            private List<float> iconSeparations;

            private Vector2 BarScale;

            private int Health;

            private Level level;

            public int Count
            {
                get
                {
                    return healthIcons.Count;
                }
            }

            public new bool Visible
            {
                get
                {
                    return healthIcons.Any(icon => icon.Visible);
                }
                set
                {
                    healthIcons.ForEach(icon => icon.Visible = value);
                }
            }

            public HealthIconList()
            {
                useSessionValues = true;
                healthIcons = new List<HealthIcon>();
            }

            public HealthIconList(EntityData entityData, int health, Vector2 barPosition, Vector2 barScale)
            {
                useSessionValues = false;
                Health = health;
                Position = barPosition;
                BarScale = barScale;
                healthIcons = new List<HealthIcon>();
                this.icons = SeparateList(entityData.Attr("healthIcons"));
                this.createAnims = SeparateList(entityData.Attr("healthIconsCreateAnim"));
                this.removeAnims = SeparateList(entityData.Attr("healthIconsCreateAnim"));
                this.iconSeparations = SeparateFloatList(entityData.Attr("healthIconsSeparation"));
                for (int i = 0; i < Health; i++)
                {
                    healthIcons.Add(new HealthIcon(BarScale,
                        icons.ElementAtOrDefault(i, icons.Last()),
                        createAnims.ElementAtOrDefault(i, createAnims.Last()),
                        removeAnims.ElementAtOrDefault(i, removeAnims.Last())
                        )
                    );
                }
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                level = scene as Level;
                if (useSessionValues)
                {
                    Health = HealthData.playerHealthVal;
                    Position = HealthData.healthBarPos;
                    BarScale = HealthData.healthIconScale;
                    icons = SeparateList(HealthData.iconSprite);
                    createAnims = SeparateList(HealthData.startAnim);
                    removeAnims = SeparateList(HealthData.endAnim);
                    iconSeparations = SeparateFloatList(HealthData.iconSeparation);
                }
            }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                DrawHealthBar();
            }

            public void DrawHealthBar()
            {
                for (int i = 0; i < Health; i++)
                {
                    level.Add(healthIcons[i]);
                    healthIcons[i].DrawIcon(Position + Vector2.UnitX * GetEffectiveSeparation(i));
                }
            }

            public void RefillHealth()
            {
                for (int i = healthIcons.Count; i < Health; i++)
                {
                    IncreaseHealth(i);
                }
            }

            public void IncreaseHealth(int i)
            {
                HealthIcon healthIcon = new HealthIcon(BarScale,
                    icons.ElementAtOrDefault(i, icons.Last()),
                    createAnims.ElementAtOrDefault(i, createAnims.Last()),
                    removeAnims.ElementAtOrDefault(i, removeAnims.Last())
                );
                healthIcons.Add(healthIcon);
                level.Add(healthIcon);
                healthIcon.DrawIcon(Position + Vector2.UnitX * GetEffectiveSeparation(i));
            }

            private float GetEffectiveSeparation(int index)
            {
                if (index == 0)
                    return 0f;
                if (index == 1)
                    return iconSeparations[0];

                float sum = iconSeparations[0];
                for (int i = 1; i < index; i++)
                {
                    sum += iconSeparations.ElementAtOrDefault(i, iconSeparations.Last());
                }
                return sum;
            }

            public void DecreaseHealth()
            {
                if (healthIcons.Count > 0)
                {
                    healthIcons[healthIcons.Count - 1].RemoveIcon();
                    healthIcons.RemoveAt(healthIcons.Count - 1);
                }
                else
                {
                    Logger.Log("Health Render", "No Health Icon to remove");
                }
            }

            public void ForEach(Action<HealthIcon> action)
            {
                healthIcons.ForEach(action);
            }

            public void Clear()
            {
                healthIcons.Clear();
            }

            public override void Removed(Scene scene)
            {
                healthIcons.ForEach((x) => x.RemoveSelf());
                base.Removed(scene);
            }
        }

        public class HealthNumber : Entity
        {
            private readonly Func<int> bossHealth;

            private Vector2 barScale;

            private readonly Color baseColor;

            private Color color;

            public HealthNumber(Vector2 barPosition, Vector2 barScale, Func<int> bossHealth, Color color)
            {
                Position = barPosition;
                this.barScale = barScale;
                this.bossHealth = bossHealth;
                this.baseColor = color;
                this.color = color;
                Tag = Tags.HUD;
            }

            public override void Update()
            {
                base.Update();
                if (color != baseColor)
                {
                    color = Color.Lerp(color, baseColor, 0.1f);
                }
            }

            public override void Render()
            {
                base.Render();
                ActiveFont.Draw(bossHealth.Invoke().ToString(), Position, new Vector2(0.5f, 0.5f), barScale, color);
            }
        }

        public class HealthBar : Entity
        {
            private readonly Func<int> bossHealth;

            private readonly float leftEdge;

            private readonly int barDir;

            private readonly Color baseColor;

            private Color color;

            private readonly float MaxWidth;

            private readonly int MaxHealth;

            public HealthBar(Vector2 barPosition, Vector2 barScale, Func<int> bossHealth, Color color, int barDir)
            {
                base.Collider = new Hitbox(barScale.X, barScale.Y);
                MaxWidth = barScale.X;
                base.Position = barPosition;
                this.barDir = barDir;
                if (barDir == -1)
                {
                    Position.X = barPosition.X - barScale.X;
                }
                else if (barDir == 0)
                {
                    Position.X = barPosition.X - barScale.X / 2;
                }
                leftEdge = Position.X;
                this.bossHealth = bossHealth;
                this.baseColor = color;
                this.color = color;

                MaxHealth = bossHealth.Invoke();
                Tag = Tags.HUD;
            }

            public override void Update()
            {
                base.Update();
                if (color != baseColor)
                {
                    color = Color.Lerp(color, baseColor, 0.1f);
                }
                Collider.Width = MaxWidth * bossHealth() / MaxHealth;
                if (barDir == -1)
                {
                    Position.X = leftEdge + (MaxWidth - Collider.Width);
                }
                else if (barDir == 0)
                {
                    Position.X = leftEdge + (MaxWidth - Collider.Width) / 2;
                }
            }

            public override void Render()
            {
                Draw.Rect(Collider, color);
                base.Render();
            }
        }

        public static List<string> SeparateList(string listString)
        {
            return listString.Replace(" ", string.Empty).Split([',']).ToList();
        }

        public static List<float> SeparateFloatList(string listString)
        {
            return listString.Replace(" ", string.Empty).Split([',']).Select(float.Parse).ToList();
        }
    }
}
