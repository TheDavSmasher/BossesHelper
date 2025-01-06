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
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/PlayerSavePoint")]
    public class GlobalSavePoint : Actor
    {
        private readonly GlobalSavePointChanger Changer;

        public readonly Sprite Sprite;

        private TalkComponent talker;

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
            onInteract = LoadLuaFile(filename, "getSavePointData", dict)?.ElementAtOrDefault(0);
        }

        public GlobalSavePoint(EntityData entityData, Vector2 offset)
            : base(entityData.Position + offset)
        {
            Add(Changer = new(entityData.Level, entityData.Nodes.First(), entityData.Enum("respawnType", Player.IntroTypes.Respawn)));
            filepath = entityData.String("luaFile");
            string spriteName = entityData.String("savePointSprite");
            GFX.SpriteBank.TryCreate(spriteName, out Sprite);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            talker = new TalkComponent(new Rectangle(0, 0, Sprite.width, 8), Changer.spawnPoint - Position, OnTalk)
            {
                Enabled = true,
                PlayerMustBeFacing = false
            };
            Add(talker);
            if (scene.GetPlayer() is Player player)
                this.ReadSavePointFunction(filepath, player);
        }

        public void OnTalk(Player _)
        {
            Changer.Update();
            Add(new Coroutine(SaveRoutine()));
        }

        private IEnumerator SaveRoutine()
        {
            yield return onInteract?.ToIEnumerator();
        }
    }
}
