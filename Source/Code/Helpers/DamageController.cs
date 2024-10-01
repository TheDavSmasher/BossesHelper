using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    [Tracked(false)]
    public class DamageController : Entity
    {
        private float damageCooldown;

        private readonly float baseCooldown;

        public int health;

        private Level level;

        internal DamageController()
        {
            if (BossesHelperModule.Session.healthData.globalController)
                AddTag(Tags.Global);
            baseCooldown = BossesHelperModule.Session.healthData.damageCooldown;
            health = BossesHelperModule.Session.healthData.playerHealthVal;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public void TakeDamage(Vector2 origin, int amount = 1, bool silent = false)
        {
            if (damageCooldown > 0 || SaveData.Instance.Assists.Invincible)
            {
                return;
            }
            if (level.InCutscene)
            {
                return;
            }
            damageCooldown = baseCooldown;
            health -= amount;
            if (!silent)
            {
                level.Shake();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                level.Flash(Color.Red * 0.3f);
            }
            Player entity = Engine.Scene.Tracker.GetEntity<Player>();
            if (entity == null)
            {
                return;
            }
            if (health > 0)
            {
                if (!silent)
                {
                    Audio.Play("event:/char/madeline/predeath");
                    Add(new Coroutine(PlayerStagger(entity, origin)));
                    Add(new Coroutine(PlayerInvincible(entity)));
                }
            }
            else
            {
                entity.Die((entity.Position - origin).SafeNormalize());
            }
            if (BossesHelperModule.Session.mapHealthBar != null)
            {
                for (int i = 0; i < amount; i++)
                {
                    BossesHelperModule.Session.mapHealthBar.DecreaseHealth();
                }
            }
            else
            {
                Logger.Log("Bosses Helper", "No Health Bar has been initialized");
            }
        }

        public void RecoverHealth(int amount = 1)
        {
            health += amount;
            for (int i = 0; i < amount; i++)
            {
                BossesHelperModule.Session.mapHealthBar.IncreaseHealth();
            }
        }

        private IEnumerator PlayerStagger(Player player, Vector2 bounce)
        {
            if (bounce != Vector2.Zero)
            {
                Celeste.Freeze(0.05f);
                yield return null;
                Vector2 from = player.Position;
                Vector2 to = new Vector2(from.X + (float)((!(bounce.X < 0f)) ? 1 : (-1)) * 20f, from.Y - 5f);
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.2f, start: true);
                Add(tween);
                tween.OnUpdate = delegate (Tween t)
                {
                    Vector2 val = from + (to - from) * t.Eased;
                    player.MoveToX(val.X);
                    player.MoveToY(val.Y);
                    player.Sprite.Rotation = (float)(Math.Floor(t.Eased * 4f) * 6.2831854820251465);
                };
                yield return tween.Duration;
                tween.Stop();
            }
        }

        private IEnumerator PlayerInvincible(Player player)
        {
            int times = 2;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, damageCooldown, start: true);
            Add(tween);
            tween.OnUpdate = delegate
            {
                if (base.Scene.OnInterval(0.02f))
                {
                    if (times <= 0)
                    {
                        player.Sprite.Visible = false;
                        player.Hair.Visible = false;
                        times = 2;
                    }
                    else
                    {
                        player.Sprite.Visible = true;
                        player.Hair.Visible = true;
                        times--;
                    }
                }
            };
            yield return tween.Duration;
            tween.Stop();
            player.Sprite.Visible = true;
            player.Hair.Visible = true;
        }

        public override void Update()
        {
            base.Update();
            if (damageCooldown > 0)
            {
                damageCooldown -= Engine.DeltaTime;
            }
        }
    }
}
