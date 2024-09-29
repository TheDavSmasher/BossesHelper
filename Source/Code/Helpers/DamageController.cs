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

        public int health;

        public DamageController()
        {
            AddTag(Tags.Global);
            health = BossesHelperModule.playerHealthBar.health;
        }

        public void TakeDamage(Vector2 origin, int amount = 1)
        {
            if (damageCooldown > 0 || SaveData.Instance.Assists.Invincible)
            {
                return;
            }
            Level level = SceneAs<Level>();
            if (level.InCutscene)
            {
                return;
            }
            damageCooldown = 1f;
            health -= amount;
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            level.Flash(Color.Red * 0.3f);
            Player entity = Engine.Scene.Tracker.GetEntity<Player>();
            if (entity == null)
            {
                return;
            }
            if (health > 0)
            {
                Audio.Play("event:/char/madeline/predeath");
                Add(new Coroutine(PlayerStagger(entity, origin)));
                Add(new Coroutine(PlayerInvincible(entity)));
            }
            else
            {
                entity.Die((entity.Center - origin).SafeNormalize());
            }
            if (BossesHelperModule.playerHealthBar != null)
            {
                for (int i = 0; i < amount; i++)
                {
                    BossesHelperModule.playerHealthBar.DecreaseHealth();
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
                BossesHelperModule.playerHealthBar.IncreaseHealth();
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
