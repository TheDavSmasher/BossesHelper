using System;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked]
    internal class BadelineSidekick : Entity
    {
        public Follower Follower { get; private set; }

        private float oldX;

        private enum SidekickSprite
        {
            Dummy,
            Boss,
            Custom
        }

        private SidekickSprite currentSprite;

        public Sprite ActiveSprite
        {
            get
            {
                return currentSprite switch
                {
                    SidekickSprite.Boss => Boss,
                    SidekickSprite.Custom => Custom,
                    _ => Dummy,
                };
            }
        }

        private readonly PlayerSprite Dummy;

        private readonly PlayerHair DummyHair;

        private readonly Sprite Boss;

        private readonly Sprite Custom;

        private readonly SineWave Wave;

        private readonly VertexLight Light;

        private readonly SoundSource laserSfx;

        public Vector2 BeamOrigin
        {
            get
            {
                return base.Center + Boss.Position + new Vector2(0f, -14f);
            }
        }

        private bool CanAttack;

        private bool FreezeOnAttack;

        private readonly float sidekickCooldown;

        public BadelineSidekick(Vector2 position, bool freezeOnAttack, float cooldown) : base(position)
        {
            Dummy = new PlayerSprite(PlayerSpriteMode.Badeline);
            Dummy.Scale.X = -1f;
            Dummy.Play("fallslow");
            DummyHair = new(Dummy)
            {
                Color = BadelineOldsite.HairColor,
                Border = Color.Black,
                Facing = Facings.Left
            };
            Boss = GFX.SpriteBank.Create("badeline_boss");
            Boss.OnFrameChange = (string anim) =>
            {
                if (anim == "idle" && Boss.CurrentAnimationFrame == 18 && Boss.Visible)
                {
                    Audio.Play("event:/char/badeline/boss_idle_air", Position);
                }
            };
            //Custom = GFX.SpriteBank.Create("badeline_sidekick");
            //PlayerSprite.CreateFramesMetadata("badeline_sidekick");
            Add(DummyHair);
            Add(Dummy);
            Add(Boss);
            /*Add(Custom);*/
            Add(Wave = new SineWave(0.25f, 0f));
            Wave.OnUpdate = (float f) =>
            {
                ActiveSprite.Position = Vector2.UnitY * f * 2f;
            };
            Add(Light = new VertexLight(new Vector2(0f, -8f), Color.PaleVioletRed, 1f, 20, 60));

            Add(Follower = new Follower());
            Follower.PersistentFollow = true;
            AddTag(Tags.Persistent);
            SetActiveSpriteTo(SidekickSprite.Dummy);
            Add(laserSfx = new SoundSource());
            FreezeOnAttack = freezeOnAttack;
            sidekickCooldown = cooldown;
            CanAttack = true;
        }

        private IEnumerator Beam()
        {
            if (FreezeOnAttack)
            {
                Follower.MoveTowardsLeader = false;
            }
            laserSfx.Play("event:/char/badeline/boss_laser_charge");
            if (ActiveSprite != Boss)
            {
                SetActiveSpriteTo(SidekickSprite.Boss);
            }
            ActiveSprite.Play("attack2Begin", true);
            yield return 0.1f;
            Level level = SceneAs<Level>();
            SidekickTarget target = level.Tracker.GetNearestEntity<SidekickTarget>(BeamOrigin);
            if (target != null)
            {
                level.CreateAndAdd<SidekickBeam>().Init(this, target);
            }
            yield return 0.9f;
            ActiveSprite.Play("attack2Lock", true);
            yield return 0.5f;
            laserSfx.Stop();
            Audio.Play("event:/char/badeline/boss_laser_fire", Position);
            ActiveSprite.Play("attack2Recoil");
            yield return 0.5f;
            Follower.MoveTowardsLeader = true;
            yield return Reload();
        }

        private IEnumerator Reload()
        {
            Vanish();
            yield return sidekickCooldown;
            Appear();
            CanAttack = true;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = SceneAs<Level>();
            if (level.Tracker.GetEntities<SidekickTarget>().Count == 0)
            {
                RemoveSelf();
                return;
            }
            Player entity = level.Tracker.GetEntity<Player>();
            entity?.Leader.GainFollower(Follower);
        }

        public override void Update()
        {
            Level level = SceneAs<Level>();
            Player entity = level.Tracker.GetEntity<Player>();
            if (entity != null && (Follower.Leader == null || Follower.Leader.Entity != entity))
            {
                entity.Leader.GainFollower(Follower);
            }
            if ((oldX - X) * ActiveSprite.Scale.X > 0)
            {
                ActiveSprite.Scale.X *= -1;
                DummyHair.Facing = (Facings)Math.Sign(ActiveSprite.Scale.X);
            }
            if (BossesHelperModule.Settings.SidekickLaserBind.Pressed && CanAttack)
            {
                CanAttack = false;
                Add(new Coroutine(Beam()));
            }
            oldX = X;
            base.Update();
            Light.Position = ActiveSprite.Position + new Vector2(0f, -10f);
        }

        public override void Render()
        {
            Vector2 renderPosition = ActiveSprite.RenderPosition;
            ActiveSprite.RenderPosition = ActiveSprite.RenderPosition.Floor();
            base.Render();
            ActiveSprite.RenderPosition = renderPosition;
        }

        private void SetActiveSpriteTo(SidekickSprite value)
        {
            if (ActiveSprite != null)
            {
                currentSprite = value;
            }
            switch (value)
            {
                case SidekickSprite.Dummy:
                    Dummy.Visible = true;
                    DummyHair.Visible = true;
                    Boss.Visible = false;
                    /*Custom.Visible = false;*/
                    break;
                case SidekickSprite.Boss:
                    Boss.Visible = true;
                    Dummy.Visible = false;
                    DummyHair.Visible = false;
                    /*Custom.Visible = false;*/
                    break;
                case SidekickSprite.Custom:
                    /*Custom.Visible = true;*/
                    Boss.Visible = false;
                    Dummy.Visible = false;
                    DummyHair.Visible = false;
                    break;
            }
        }

        public void Appear()
        {
            Level level = SceneAs<Level>();
            level.Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
            level.Particles.Emit(BadelineOldsite.P_Vanish, 12, base.Center, Vector2.One * 6f);
            SetActiveSpriteTo(SidekickSprite.Dummy);
        }

        public void Vanish()
        {
            Level level = SceneAs<Level>();
            level.Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
            level.Particles.Emit(BadelineOldsite.P_Vanish, 12, base.Center, Vector2.One * 6f);
            SetActiveSpriteTo(SidekickSprite.Custom);
        }
    }
}
