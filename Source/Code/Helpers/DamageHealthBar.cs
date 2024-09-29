using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    [Tracked(false)]
    public class DamageHealthBar : Entity
    {
        private class HealthIcon : Entity
        {
            private readonly  Sprite icon;

            private readonly string createAnim;

            private readonly string removeAnim;

            public HealthIcon(string spriteName, string createAnim, string removeAnim)
            {
                Add(icon = GFX.SpriteBank.Create(spriteName));
                base.Tag = Tags.HUD;
                AddTag(Tags.Global);
                this.createAnim = createAnim;
                this.removeAnim = removeAnim;
            }

            public void DrawIcon(Vector2 position)
            {
                Position = position;
                Add(new Coroutine(DrawRoutine()));
            }

            private IEnumerator DrawRoutine()
            {
                if (!string.IsNullOrEmpty(createAnim) && icon.Has(createAnim))
                    icon.Play(createAnim);
                yield return 0.32f;
            }

            public void RemoveIcon()
            {
                Add(new Coroutine(RemoveRoutine()));
            }

            private IEnumerator RemoveRoutine()
            {
                if (!string.IsNullOrEmpty(removeAnim) && icon.Has(removeAnim))
                    icon.Play(removeAnim);
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

        private readonly string spritePath;

        private readonly string spriteCreate;

        private readonly string spriteRemove;

        private readonly float iconSpace;

        public int health;

        internal DamageHealthBar(Vector2 pos, int health, string iconSprite, string createIconAnim, string removeIconAnim, float iconSeparation)
            : base(pos)
        {
            healthIcons = new List<HealthIcon>();
            spritePath = iconSprite;
            spriteCreate = createIconAnim;
            spriteRemove = removeIconAnim;
            for (int i = 0; i < health; i++)
            {
                healthIcons.Add(new HealthIcon(spritePath, spriteCreate, spriteRemove));
            }
            this.health = health;
            iconSpace = iconSeparation;
            base.Tag = Tags.HUD;
            AddTag(Tags.Global);
        }

        internal DamageHealthBar(EntityData data)
            : this(new Vector2(data.Float("screenX"), data.Float("screenY")), data.Int("playerHealth"), data.Attr("healthIcon"),
                  data.Attr("healthIconCreateAnim"), data.Attr("healthIconRemoveAnim"), data.Float("healthIconSeparation"))
        {
            BossesHelperModule.healthBarPos = Position;
            BossesHelperModule.playerHealthVal = health;
            BossesHelperModule.iconSprite = spritePath;
            BossesHelperModule.startAnim = spriteCreate;
            BossesHelperModule.endAnim = spriteRemove;
            BossesHelperModule.iconSeparation = iconSpace;
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
                healthIcons[i].DrawIcon(Position + Vector2.UnitX * iconSpace * i);
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
            HealthIcon healthIcon = new HealthIcon(spritePath, spriteCreate, spriteRemove);
            healthIcons.Add(healthIcon);
            level.Add(healthIcon);
            healthIcon.DrawIcon(Position + Vector2.UnitX * iconSpace * (healthIcons.Count - 1));
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
