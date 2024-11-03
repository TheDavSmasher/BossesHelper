using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [CustomEntity("BossesHelper/BossHealthBar")]
    public class BossHealthBar : Entity
    {
        private Vector2 BarPosition;

        private Vector2 BarScale;

        private Func<int> BossHealth;

        private class HealthIcon : Entity
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

        private class HealthNumber : Entity
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

        private class HealthBar : Entity
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

            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Collider.Render(camera, Collidable ? Color.Red : Color.DarkRed);
            }
        }

        private enum BarTypes
        {
            Icons,
            BarLeft,
            BarRight,
            BarCentered,
            Countdown
        }

        private Level level;

        private readonly BarTypes barType;

        private List<HealthIcon> healthIcons;

        private HealthNumber healthNumber;

        private HealthBar healthBar;

        private readonly EntityData entityData;

        private int Health => BossHealth.Invoke();

        public bool HealthVisible
        {
            get
            {
                return barType switch
                {
                    BarTypes.Icons => healthIcons.Any(icon => icon.Visible),
                    BarTypes.Countdown => healthNumber.Visible,
                    _ => healthBar.Visible
                };
            }
            set
            {
                switch (barType)
                {
                    case BarTypes.Icons:
                        healthIcons.ForEach(icon => icon.Visible = value);
                        break;
                    case BarTypes.Countdown:
                        healthNumber.Visible = value;
                        break;
                    default:
                        healthBar.Visible = value;
                        break;
                }
            }
        }

        public BossHealthBar(EntityData data, Vector2 _)
        {
            this.entityData = data;
            BarPosition = (Position = new Vector2(data.Float("healthBarX"), data.Float("healthBarY")));
            BarScale = new Vector2(data.Float("healthScaleX", 1f), data.Float("healthScaleY", 1f));
            barType = data.Enum<BarTypes>("barType", BarTypes.BarLeft);
            Tag = Tags.HUD;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = SceneAs<Level>();
            BossHealthComponent component = SceneAs<Level>().Tracker.GetNearestComponent<BossHealthComponent>(entityData.Nodes[0]);
            if (component == null)
            {
                RemoveSelf();
                return;
            }
            BossHealth = component.Health;
            Color baseColor = entityData.HexColor("baseColor", Color.White);
            switch (barType)
            {
                case BarTypes.Icons:
                    healthIcons = new List<HealthIcon>();
                    for (int i = 0; i < Health; i++)
                    {
                        healthIcons.Add(new HealthIcon(BarScale,
                            entityData.Attr("healthIcons"),
                            entityData.Attr("healthIconsCreateAnim"),
                            entityData.Attr("healthIconsRemoveAnim")));
                    }
                    DrawHealthBar();
                    break;
                case BarTypes.Countdown:
                    level.Add(healthNumber = new HealthNumber(BarPosition, BarScale, BossHealth, baseColor));
                    break;
                default:
                    int barAnchor = barType == BarTypes.BarCentered ? 0 : barType == BarTypes.BarLeft ? -1 : 1;
                    level.Add(healthBar = new HealthBar(BarPosition, BarScale, BossHealth, baseColor, barAnchor));
                    break;
            }
            HealthVisible = entityData.Bool("startVisible");
        }

        public override void Update()
        {
            base.Update();
            if (barType == BarTypes.Icons)
            {
                int lostHealth = healthIcons.Count - BossHealth();
                if (lostHealth > 0)
                {
                    for (int i = 0; i < lostHealth; i++)
                    {
                        DecreaseHealth();
                    }
                }
            }
        }

        private void DrawHealthBar()
        {
            float separation = entityData.Float("healthIconsSeparation", 1f);
            for (int i = 0; i < Health; i++)
            {
                level.Add(healthIcons[i]);
                healthIcons[i].DrawIcon(Position + Vector2.UnitX * separation * i);
            }
        }

        public void RefillHealth()
        {
            for (int i = healthIcons.Count; i < Health; i++)
            {
                IncreaseHealth();
            }
        }

        public void IncreaseHealth()
        {
            HealthIcon healthIcon = new HealthIcon(BarScale,
                entityData.Attr("healthIcons"),
                entityData.Attr("healthIconsCreateAnim"),
                entityData.Attr("healthIconsRemoveAnim"));
            healthIcons.Add(healthIcon);
            level.Add(healthIcon);
            healthIcon.DrawIcon(Position + Vector2.UnitX * entityData.Float("healthIconsSeparation", 1f) * (healthIcons.Count - 1));
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

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            healthIcons?.ForEach(x => x.RemoveSelf());
            healthIcons?.Clear();
            healthBar?.RemoveSelf();
            healthNumber?.RemoveSelf();
        }
    }
}
