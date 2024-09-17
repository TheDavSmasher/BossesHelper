using Celeste.Mod.BossesHelper.Code.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Entities.BossController;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    public class BossPuppet : Entity
    {
        public struct HitboxMedatata(List<Collider> baseHitboxes, Hitbox bounceHitbox, Vector2 target, float radius)
        {
            public List<Collider> baseHitboxes = baseHitboxes;

            public Hitbox bounceHitbox = bounceHitbox;

            public Vector2 targetOffset = target;

            public float targetRadius = radius;

            public readonly bool UseDefaultBase
            {
                get
                {
                    return baseHitboxes == null || baseHitboxes.Count == 0;
                }
            }

            public readonly bool UseDefaultBounce
            {
                get
                {
                    return bounceHitbox == null;
                }
            }
        }

        private Sprite Sprite;

        private readonly string SpriteName;

        private readonly bool DynamicFacing;

        private readonly bool MirrorSprite;

        private BossController.MoveModes MoveMode;

        private BossController.HurtModes HurtMode;

        private readonly Vector2[] nodes;

        private readonly Action OnHit;

        private float bossHitCooldown;

        private int facing;

        private Level Level;

        public BossPuppet(EntityData data, Vector2 offset, Action onHit, HitboxMedatata hitboxMedatata) : base(data.Position + offset)
        {
            nodes = data.Nodes;
            SpriteName = data.Attr("bossSprite");
            DynamicFacing = data.Bool("dynamicFacing");
            MirrorSprite = data.Bool("mirrorSprite");
            bossHitCooldown = data.Float("bossHitCooldown", 0.5f);
            nodes = data.Nodes;
            MoveMode = GetMoveMode(data.Attr("moveMode"));
            HurtMode = GetHurtMode(data.Attr("hurtMode"));
            OnHit = onHit;
            if (!string.IsNullOrEmpty(SpriteName))
            {
                Sprite = GFX.SpriteBank.Create(SpriteName);
                Sprite.Scale = Vector2.One;
                SetHitboxesAndColliders(hitboxMedatata);
                facing = MirrorSprite ? -1 : 1;
                Add(Sprite);
                PlayBossAnim("idle");
                if (data.Bool("killOnContact"))
                {
                    Add(new PlayerCollider(KillOnContact));
                }
            }
        }

        private void SetHitboxesAndColliders(HitboxMedatata hitboxMedatata)
        {
            if (hitboxMedatata.UseDefaultBase)
            {
                base.Collider = new Hitbox(Sprite.Width, Sprite.Height, Sprite.Width * -0.5f, Sprite.Height * -0.5f);
            }
            else if (hitboxMedatata.baseHitboxes.Count > 1)
            {
                base.Collider = new ColliderList(hitboxMedatata.baseHitboxes.ToArray());
            }
            else
            {
                base.Collider = hitboxMedatata.baseHitboxes[0];
            }
            switch (HurtMode)
            {
                case HurtModes.HeadBonk:
                    if (hitboxMedatata.UseDefaultBounce)
                    {
                        Add(new PlayerCollider(OnPlayerBounce, new Hitbox(base.Collider.Width, 6f, Sprite.Width * -0.5f, Sprite.Height * -0.5f)));
                    }
                    else
                    {
                        Add(new PlayerCollider(OnPlayerBounce, hitboxMedatata.bounceHitbox));
                    }
                    break;
                case HurtModes.SidekickAttack:
                    Add(new SidekickTargetComp(SpriteName, Position, hitboxMedatata.targetOffset, OnHit, hitboxMedatata.targetRadius));
                    break;
                case HurtModes.PlayerDash:
                    Add(new PlayerCollider(OnPlayerDash));
                    break;
                default: //PlayerContact 
                    Add(new PlayerCollider(OnPlayerContact));
                    break;
            }
        }

        private static MoveModes GetMoveMode(string moveMode)
        {
            return moveMode switch
            {
                "static" => MoveModes.Static,
                "screenEdge" => MoveModes.ScreenEdge,
                "playerPos" => MoveModes.PlayerPos,
                "playerScreenEdge" => MoveModes.PlayerScreenEdge,
                "freeroam" => MoveModes.Freeroam,
                _ => MoveModes.Nodes
            };
        }

        private static HurtModes GetHurtMode(string moveMode)
        {
            return moveMode switch
            {
                "playerDash" => HurtModes.PlayerDash,
                "headBonk" => HurtModes.HeadBonk,
                "sidekickAttack" => HurtModes.SidekickAttack,
                _ => HurtModes.PlayerContact
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                if (DynamicFacing)
                {
                    if (facing == -1 && PositionOver_Quarter(entity.X, !MirrorSprite))
                    {
                        facing = 1;
                    }
                    else if (facing == 1 && PositionOver_Quarter(entity.X, MirrorSprite))
                    {
                        facing = -1;
                    }
                }
                
            }
            if (bossHitCooldown > 0)
            {
                bossHitCooldown -= Engine.DeltaTime;
            }
        }

        private bool PositionOver_Quarter(float pos, bool left)
        {
            if (left)
            {
                return pos < base.X - base.Collider.Width / 4;
            }
            return pos > base.X + base.Collider.Width / 4;
        }

        public override void Render()
        {
            base.Render();
            if (Sprite != null)
            {
                Sprite.Scale.X = facing;
            }
        }

        public void PlayBossAnim(string name)
        {
            if (Sprite != null)
            {
                if (Sprite.Has(name))
                {
                    Sprite.Play(name);
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "BossesHelper/BossPuppet", "Animation specified does not exist!");
                }
            }
        }

        public void SetBossHitCooldown(float timer)
        {
            bossHitCooldown = timer;
        }

        private void KillOnContact(Player player)
        {
            player.Die((player.Position - Position).SafeNormalize());
        }

        private void OnPlayerBounce(Player player)
        {
            if (bossHitCooldown <= 0)
            {
                Audio.Play("event:/game/general/thing_booped", Position);
                Celeste.Freeze(0.2f);
                player.Bounce(base.Top + 2f);
                OnHit.Invoke();
            }
        }

        private void OnPlayerDash(Player player)
        {
            if (bossHitCooldown <= 0 && player.DashAttacking && player.Speed != Vector2.Zero)
            {
                OnHit.Invoke();
            }
        }

        private void OnPlayerContact(Player player)
        {
            if (bossHitCooldown <= 0)
            {
                OnHit.Invoke();
            }
        }

        public IEnumerator MoveSequence()
        {
            //TODO finish
            yield return null;
        }
    }
}
