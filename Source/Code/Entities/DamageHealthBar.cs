using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    public class DamageHealthBar : Entity
    {
        private static BossesHelperSession.HealthSystemData healthData => BossesHelperModule.Session.healthData;

        private class HealthIcon : Entity
        {
            private readonly Sprite icon;

            public HealthIcon()
            {
                Add(icon = GFX.SpriteBank.Create(BossesHelperModule.Session.healthData.iconSprite));
                icon.Scale = BossesHelperModule.Session.healthData.healthIconScale;
                Tag = Tags.HUD;
                if (BossesHelperModule.Session.healthData.globalController)
                    AddTag(Tags.Global);
            }

            public void DrawIcon(Vector2 position)
            {
                Position = position;
                Add(new Coroutine(DrawRoutine()));
            }

            private IEnumerator DrawRoutine()
            {
                if (!string.IsNullOrEmpty(BossesHelperModule.Session.healthData.startAnim) && icon.Has(BossesHelperModule.Session.healthData.startAnim))
                    icon.Play(BossesHelperModule.Session.healthData.startAnim);
                yield return 0.32f;
            }

            public void RemoveIcon()
            {
                Add(new Coroutine(RemoveRoutine()));
            }

            private IEnumerator RemoveRoutine()
            {
                if (!string.IsNullOrEmpty(BossesHelperModule.Session.healthData.endAnim) && icon.Has(BossesHelperModule.Session.healthData.endAnim))
                    icon.Play(BossesHelperModule.Session.healthData.endAnim);
                yield return 0.88f;
                RemoveSelf();
            }

            public override void Render()
            {
                base.Render();
                icon.Visible = !Scene.Paused;
            }
        }

        private Level level;

        public bool HealthVisible
        {
            get
            {
                return healthIcons.Any(icon => icon.Visible);
            }
            set
            {
                foreach (HealthIcon icon in healthIcons)
                {
                    icon.Visible = value;
                }
            }
        }

        private readonly List<HealthIcon> healthIcons;

        private static int Health => healthData.playerHealthVal;

        internal DamageHealthBar()
        {
            Position = healthData.healthBarPos;
            healthIcons = new List<HealthIcon>();
            Tag = Tags.HUD;
            HealthVisible = healthData.startVisible;
            if (healthData.globalController)
                AddTag(Tags.Global);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            healthIcons.Clear();
            for (int i = 0; i < Health; i++)
            {
                healthIcons.Add(new HealthIcon());
            }
            DrawHealthBar();
        }

        private void DrawHealthBar()
        {
            for (int i = 0; i < Health; i++)
            {
                level.Add(healthIcons[i]);
                healthIcons[i].DrawIcon(Position + Vector2.UnitX * healthData.iconSeparation * i);
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
            HealthIcon healthIcon = new HealthIcon();
            healthIcons.Add(healthIcon);
            level.Add(healthIcon);
            healthIcon.DrawIcon(Position + Vector2.UnitX * healthData.iconSeparation * (healthIcons.Count - 1));
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
            healthIcons.ForEach(x => x.RemoveSelf());
            healthIcons.Clear();
        }
    }
}
