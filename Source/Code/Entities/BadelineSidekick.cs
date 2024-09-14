using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    internal class BadelineSidekick : Entity
    {
        public BadelineDummy Sidekick { get; private set; }

        public Follower Follower { get; private set; }

        private float oldX;

        public BadelineSidekick(Vector2 position) : base(position)
        {
            Sidekick = new BadelineDummy(position);
            Sidekick.Add(Follower = new Follower());
            Follower.PersistentFollow = true;
            AddTag(Tags.Persistent);
            Sidekick.AddTag(Tags.Persistent);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            (scene as Level).Add(Sidekick);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player entity = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (entity != null)
            {
                entity.Leader.GainFollower(Follower);
            }
        }

        public override void Update()
        {
            Player entity = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (entity != null && (Follower.Leader == null || Follower.Leader.Entity != entity))
            {
                entity.Leader.GainFollower(Follower);
            }
            if ((oldX - Sidekick.X) * Sidekick.Sprite.Scale.X > 0)
            {
                Sidekick.Sprite.Scale.X *= -1;
            }
            oldX = Sidekick.X;
            base.Update();
        }
    }
}
