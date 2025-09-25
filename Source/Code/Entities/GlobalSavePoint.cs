﻿using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;
using static Celeste.Mod.BossesHelper.Code.Helpers.UserFileReader;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
	[Tracked(false)]
	[CustomEntity("BossesHelper/PlayerSavePoint")]
	public class GlobalSavePoint : Entity, ILuaLoader
	{
		private readonly GlobalSavePointChanger Changer;

		public readonly Sprite Sprite;

		private readonly Rectangle talkerRect;

		private readonly Vector2 talkerOffset;

		private readonly string filepath;

		private LuaFunction onInteract;

		public LuaCommand Command => ("getSavePointData", 1);

		public List<LuaTableItem> Values { get; init; }

		public GlobalSavePoint(EntityData entityData, Vector2 offset)
			: base(entityData.Position + offset)
		{
			Add(Changer = new(entityData.Level, entityData.Nodes.FirstOrDefault(Position),
				entityData.Enum("respawnType", Player.IntroTypes.Respawn)));
			Values = [
				("savePoint", this),
				("spawnPoint", Changer.spawnPoint)
			];
			filepath = entityData.String("luaFile");
			string spriteName = entityData.String("savePointSprite");
			if (GFX.SpriteBank.TryCreate(spriteName, out Sprite))
				Add(Sprite);
			talkerRect = new(entityData.Int("rectXOffset"),
				entityData.Int("rectYOffset"), entityData.Int("rectWidth"), 8);
			talkerOffset = new(entityData.Float("talkerXOffset"), entityData.Float("talkerYOffset"));
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Add(new TalkComponent(talkerRect, talkerOffset, OnTalk)
			{
				Enabled = true,
				PlayerMustBeFacing = false
			});
			onInteract = ReadLuaFilePath(filepath, this.LoadFile)[0];
		}

		public void OnTalk(Player _)
		{
			Changer.Update();
			Add(new LuaCoroutineComponent(onInteract));
		}
	}
}
