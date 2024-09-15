using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
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
                switch (currentSprite)
                {
                    case SidekickSprite.Boss : return Boss;
                    case SidekickSprite.Custom : return Custom;
                    default: return Dummy;
                }
            }
        }

        private readonly PlayerSprite Dummy;

        private readonly PlayerHair DummyHair;

        private readonly Sprite Boss;

        private readonly Sprite Custom;

        public BadelineSidekick(Vector2 position) : base(position)
        {
            Dummy = new PlayerSprite(PlayerSpriteMode.Badeline);
            Dummy.Scale.X = -1f;
            DummyHair = new PlayerHair(Dummy);
            DummyHair.Color = BadelineOldsite.HairColor;
            DummyHair.Border = Color.Black;
            DummyHair.Facing = Facings.Left;
            Boss = GFX.SpriteBank.Create("badeline_boss");
            Boss.OnFrameChange = (string anim) =>
            {
                if (anim == "idle" && Boss.CurrentAnimationFrame == 18)
                {
                    Audio.Play("event:/char/badeline/boss_idle_air", Position);
                }
            };
            //Custom = GFX.SpriteBank.Create("badeline_sidekick");
            //PlayerSprite.CreateFramesMetadata("badeline_sidekick");
            Add(Dummy, DummyHair, Boss, Custom);
            Add(Follower = new Follower());
            Follower.PersistentFollow = true;
            AddTag(Tags.Persistent);
            currentSprite = SidekickSprite.Dummy;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player entity = SceneAs<Level>().Tracker.GetEntity<Player>();
            entity?.Leader.GainFollower(Follower);
        }

        public override void Update()
        {
            Player entity = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (entity != null && (Follower.Leader == null || Follower.Leader.Entity != entity))
            {
                entity.Leader.GainFollower(Follower);
            }
            if ((oldX - X) * ActiveSprite.Scale.X > 0)
            {
                ActiveSprite.Scale.X *= -1;
                DummyHair.Facing = (Facings)(((int)DummyHair.Facing) * -1);
            }
            oldX = X;
            base.Update();
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
