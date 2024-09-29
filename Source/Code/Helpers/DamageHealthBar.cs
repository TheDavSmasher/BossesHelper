using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    [Tracked(false)]
    public class DamageHealthBar : Entity
    {
        private static BossesHelperModule.HealthSystemData healthData => BossesHelperModule.healthData;

        private class HealthIcon : Entity
        {
            private readonly Sprite icon;

            public HealthIcon()
            {
                Add(icon = GFX.SpriteBank.Create(BossesHelperModule.healthData.iconSprite));
                base.Tag = Tags.HUD;
                if (BossesHelperModule.healthData.globalController)
                    AddTag(Tags.Global);
            }

            public void DrawIcon(Vector2 position)
            {
                Position = position;
                Add(new Coroutine(DrawRoutine()));
            }

            private IEnumerator DrawRoutine()
            {
                if (!string.IsNullOrEmpty(BossesHelperModule.healthData.startAnim) && icon.Has(BossesHelperModule.healthData.startAnim))
                    icon.Play(BossesHelperModule.healthData.startAnim);
                yield return 0.32f;
            }

            public void RemoveIcon()
            {
                Add(new Coroutine(RemoveRoutine()));
            }

            private IEnumerator RemoveRoutine()
            {
                if (!string.IsNullOrEmpty(BossesHelperModule.healthData.endAnim) && icon.Has(BossesHelperModule.healthData.endAnim))
                    icon.Play(BossesHelperModule.healthData.endAnim);
                yield return 0.88f;
                RemoveSelf();
            }

            public override void Render()
            {
                base.Render();
                icon.Visible = !base.Scene.Paused;
            }
        }

        private Level level;

        private readonly List<HealthIcon> healthIcons;

        public int health;

        internal DamageHealthBar()
            : base(BossesHelperModule.healthData.healthBarPos)
        {
            healthIcons = new List<HealthIcon>();
            Ctor();
        }

        internal DamageHealthBar(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            BossesHelperModule.healthData.healthBarPos = Position;
            BossesHelperModule.healthData.iconSprite = data.Attr("healthIcon");
            BossesHelperModule.healthData.startAnim = data.Attr("healthIconCreateAnim");
            BossesHelperModule.healthData.endAnim = data.Attr("healthIconRemoveAnim");
            BossesHelperModule.healthData.iconSeparation = data.Float("healthIconSeparation");
            BossesHelperModule.healthData.globalController = data.Bool("isGlobal");
            BossesHelperModule.healthData.playerHealthVal = health = data.Int("playerHealth");
            BossesHelperModule.healthData.damageCooldown = data.Float("damageCooldown", 1f);

            healthIcons = new List<HealthIcon>();
            Ctor();
        }

        private void Ctor()
        {
            for (int i = 0; i < health; i++)
            {
                healthIcons.Add(new HealthIcon());
            }
            base.Tag = Tags.HUD;
            if (healthData.globalController)
                AddTag(Tags.Global);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = SceneAs<Level>();
            DrawHealthBar();
        }

        private void DrawHealthBar()
        {
            for (int i = 0; i < health; i++)
            {
                level.Add(healthIcons[i]);
                healthIcons[i].DrawIcon(Position + Vector2.UnitX * healthData.iconSeparation * i);
            }
        }

        public void RefillHealth()
        {
            for(int i = 0; i < health; i++)
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
