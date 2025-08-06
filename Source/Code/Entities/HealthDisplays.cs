using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monocle;
using Microsoft.Xna.Framework;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    internal static class HealthDisplays
    {
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
                      SeparateList(HealthData.endAnim), SeparateFloatList(HealthData.iconSeparation), HealthData.removeOnDamage)
            { }

            public HealthIconList(EntityData entityData, Vector2 barPosition, Vector2 barScale, Func<int> getHealth)
                : this(barPosition, barScale, getHealth, false, SeparateList(entityData.Attr("healthIcons")),
                      SeparateList(entityData.Attr("healthIconsCreateAnim")), SeparateList(entityData.Attr("healthIconsCreateAnim")),
                      SeparateFloatList(entityData.Attr("healthIconsSeparation")), entityData.Bool("removeOnDamage"))
            { }

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
    }
}
