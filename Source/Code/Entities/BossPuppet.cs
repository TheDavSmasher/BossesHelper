using Monocle;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Collections;
using static Celeste.Mod.BossesHelper.Code.Entities.BossController;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    public class BossPuppet : Entity
    {
        private Sprite Sprite;

        private readonly string SpriteName;

        private readonly bool DynamicFacing;

        private readonly bool MirrorSprite;

        private BossController.MoveModes MoveMode;

        private BossController.HurtModes HurtMode;

        private readonly Vector2[] nodes;

        private int facing;

        private Level Level;

        private Dictionary<string, SoundSource> AllSfx;

        public BossPuppet(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            nodes = data.Nodes;
            SpriteName = data.Attr("bossSprite");
            DynamicFacing = data.Bool("dynamicFacing");
            MirrorSprite = data.Bool("mirrorSprite");
            nodes = data.Nodes;
            if (!string.IsNullOrEmpty(SpriteName))
            {
                Sprite = GFX.SpriteBank.Create(SpriteName);
                Sprite.Scale = Vector2.One;
                base.Collider = new Hitbox(Sprite.Width, Sprite.Height, Sprite.Width * -0.5f, Sprite.Height * -0.5f);
                facing = MirrorSprite ? -1 : 1;
                Add(Sprite);
                PlayBossAnim("idle");
                if (data.Bool("killOnContact"))
                {
                    Add(new PlayerCollider(KillOnContact));
                }
            }
            MoveMode = GetMoveMode(data.Attr("moveMode"));
            HurtMode = GetHurtMode(data.Attr("hurtMode"));
        }

        private static MoveModes GetMoveMode(string moveMode)
        {
            switch (moveMode)
            {
                case "static":
                    return MoveModes.Static;
                case "screenEdge":
                    return MoveModes.ScreenEdge;
                case "playerPos":
                    return MoveModes.PlayerPos;
                case "playerScreenEdge":
                    return MoveModes.PlayerScreenEdge;
                case "freeroam":
                    return MoveModes.Freeroam;
                default:
                    return MoveModes.Nodes;
            }
        }

        private static HurtModes GetHurtMode(string moveMode)
        {
            switch (moveMode)
            {
                case "playerDash": return HurtModes.PlayerDash;
                case "explosion": return HurtModes.Explosion;
                case "headBonk": return HurtModes.HeadBonk;
                case "sidekickAttack": return HurtModes.SidekickAttack;
                default: return HurtModes.PlayerContact;
            }
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

        private void KillOnContact(Player player)
        {
            player.Die((player.Position - Position).SafeNormalize());
        }

        public IEnumerator MoveSequence()
        {
            //TODO finish
            yield return null;
        }
    }
}
