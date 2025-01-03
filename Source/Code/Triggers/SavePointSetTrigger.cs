using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/SavePointSetTrigger")]
    public class SavePointSetTrigger : Trigger
    {
        private readonly EntityID ID;

        private readonly Player.IntroTypes spawnType;

        private readonly Vector2? spawnPosition;

        private readonly string flagTrigger;

        private readonly bool invertFlag;

        private readonly bool onlyOnce;

        private GlobalSavePointChanger Changer;

        public SavePointSetTrigger(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset)
        {
            this.ID = id;
            spawnType = data.Enum<Player.IntroTypes>("respawnType");
            if (data.FirstNodeNullable() is Vector2 spawn)
            {
                spawnPosition = spawn + offset;
            }
            onlyOnce = data.Bool("onlyOnce");
            flagTrigger = data.String("flagTrigger");
            invertFlag = data.Bool("invertFlag");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Session session = (scene as Level).Session;
            if (spawnPosition is Vector2 node)
            {
                Add(Changer = new(ID, node, spawnType));
            }
            else
            {
                Add(Changer = new(ID, session.GetSpawnPoint(Center), spawnType));
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (flagTrigger == null)
            {
                Changer?.Update();
                RemoveSelf();
                return;
            }
            bool flagSet = player.SceneAs<Level>().Session.GetFlag(flagTrigger);
            if (flagSet ^ invertFlag)
            {
                Changer?.Update();
                RemoveSelf();
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (onlyOnce)
                (scene as Level).Session.DoNotLoad.Add(ID);
        }
    }
}
