using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.BossesHelper.Code.Helpers.UserFileReader;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/PlayerSavePoint")]
    public class GlobalSavePoint : Actor
    {
        private readonly GlobalSavePointChanger Changer;

        private readonly Sprite savePointSprite;

        private readonly TalkComponent talker;

        private readonly string filepath;

        private LuaFunction onInteract;

        public void LoadFunction(string filename, Player player)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>
            {
                { "player", player },
                { "savePoint", this },
                { "spawnPoint", Changer.spawnPoint },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            };
            LuaFunction[] array = LoadLuaFile(filename, "getSavePointData", dict);
            onInteract = array?.ElementAtOrDefault(0);
        }

        public GlobalSavePoint(EntityData entityData, Vector2 offset)
            : base(entityData.Position + offset)
        {
            Add(Changer = new(entityData.Level, entityData.Nodes.First(), entityData.Enum("respawnType", Player.IntroTypes.Respawn)));
            filepath = entityData.String("luaFile");
            string spriteName = entityData.String("savePointSprite");
            if (!string.IsNullOrEmpty(spriteName))
            {
                Add(savePointSprite = GFX.SpriteBank.Create(spriteName));
                talker = new TalkComponent(new Rectangle(), Changer.spawnPoint + offset - Position, OnTalk)
                {
                    Enabled = true,
                    PlayerMustBeFacing = false
                };
                Add(talker);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((scene as Level).Tracker.GetEntity<Player>() is Player player)
                this.ReadSavePointFunction(filepath, player);
        }

        public void OnTalk(Player player)
        {
            Changer.Update();
            Add(new Coroutine(SaveRoutine(player)));
        }

        private IEnumerator SaveRoutine(Player player)
        {
            yield return onInteract?.LuaFunctionToIEnumerator();
            player.StateMachine.State = Player.StNormal;
        }
    }
}
