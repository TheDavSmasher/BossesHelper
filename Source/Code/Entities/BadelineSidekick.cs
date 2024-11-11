using System;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked]
    internal class BadelineSidekick : Entity
    {
        private readonly Follower Follower;

        private SidekickBeam beam;

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
                return base.Center + Boss.Position + new Vector2(0f, -20f);
            }
        }

        private bool CanAttack;

        private readonly bool FreezeOnAttack;

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
            Add(DummyHair);
            Add(Dummy);
            Add(Boss = GFX.SpriteBank.Create("badeline_boss"));
            Add(Custom = GFX.SpriteBank.Create("badeline_sidekick"));
            Add(Wave = new SineWave(0.25f, 0f));
            Wave.OnUpdate = (float f) =>
            {
                Dummy.Position = Custom.Position = Boss.Position = Vector2.UnitY * f * 2f;
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

        private IEnumerator AttackSequence()
        {
            if (FreezeOnAttack)
            {
                Follower.MoveTowardsLeader = false;
            }
            Wave.Active = false;
            Dummy.Position = Vector2.Zero;
            SetActiveSpriteTo(SidekickSprite.Custom);
            yield return ActiveSprite.PlayAnim("normal_to_boss");

            laserSfx.Play("event:/char/badeline/boss_laser_charge");
            SetActiveSpriteTo(SidekickSprite.Boss);
            ActiveSprite.Play("attack2Begin", true);
            yield return 0.1f;
            Level level = SceneAs<Level>();
            SidekickTargetCollider target = level.Tracker.GetNearestEntity<SidekickTargetCollider>(BeamOrigin);
            if (target != null)
            {
                beam = level.CreateAndAdd<SidekickBeam>().Init(this, target);
            }
            yield return 0.9f;
            ActiveSprite.Play("attack2Lock", true);
            yield return 0.5f;
            laserSfx.Stop();
            Audio.Play("event:/char/badeline/boss_laser_fire", Position);
            ActiveSprite.Play("attack2Recoil");
            yield return 0.5f;

            Follower.MoveTowardsLeader = true;
            Wave.Active = true;
            SetActiveSpriteTo(SidekickSprite.Custom);
            ActiveSprite.Play("boss_to_mini");
            yield return sidekickCooldown;

            yield return ActiveSprite.PlayAnim("mini_to_normal");
            SetActiveSpriteTo(SidekickSprite.Dummy);
            CanAttack = true;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = SceneAs<Level>();
            if (level.Tracker.GetEntities<SidekickTargetCollider>().Count == 0)
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
            if (beam != null && Boss.Visible)
            {
                if ((beam.oldX - X) * Boss.Scale.X < 0)
                {
                    Boss.Scale.X *= -1;
                }
            }
            else if ((oldX - X) * ActiveSprite.Scale.X > 0)
            {
                ActiveSprite.Scale.X *= -1;
                DummyHair.Facing = (Facings)Math.Sign(ActiveSprite.Scale.X);
            }
            if (BossesHelperModule.Settings.SidekickLaserBind.Pressed && CanAttack)
            {
                CanAttack = false;
                Add(new Coroutine(AttackSequence()));
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
                    Custom.Visible = false;
                    break;
                case SidekickSprite.Boss:
                    Boss.Visible = true;
                    Dummy.Visible = false;
                    DummyHair.Visible = false;
                    Custom.Visible = false;
                    break;
                case SidekickSprite.Custom:
                    Custom.Visible = true;
                    Boss.Visible = false;
                    Dummy.Visible = false;
                    DummyHair.Visible = false;
                    break;
            }
        }
    }
}
