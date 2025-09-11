using Celeste.Mod.BossesHelper.Code.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
	internal class AttackActor : Actor
	{
		public readonly Sprite Sprite;

		private readonly LuaFunction onCollide;

		public Vector2 Speed;

		private const float Gravity = 900f;

		public float GravityMult
		{
			get;
			set
			{
				field = value;
				effectiveGravity = value * Gravity;
			}
		}

		public bool SolidCollidable;

		public bool Grounded => Speed.Y >= 0 && OnGround();

		private readonly float maxFall;

		private float effectiveGravity;

		public AttackActor(Vector2 position, Collider attackbox, LuaFunction onPlayer, bool startCollidable,
			bool solidCollidable, string spriteName, float gravMult, float maxFall, float xScale = 1f, float yScale = 1f)
			: base(position)
		{
			Collider = attackbox;
			Collidable = startCollidable;
			SolidCollidable = solidCollidable;
			GravityMult = gravMult;
			this.maxFall = maxFall;
			if (GFX.SpriteBank.TryCreate(spriteName, out Sprite))
			{
				Sprite.Scale = new Vector2(xScale, yScale);
				Add(Sprite);
			}
			onCollide = onPlayer;
			Add(new PlayerCollider(OnPlayer));
		}

		public override void Update()
		{
			base.Update();
			//Move based on speed
			if (SolidCollidable)
			{
				MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
				MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
			}
			else
			{
				NaiveMove(Speed * Engine.DeltaTime);
			}
			//Apply gravity
			if (!Grounded)
			{
				Speed.Y = Calc.Approach(Speed.Y, maxFall, effectiveGravity * Engine.DeltaTime);
			}
		}

		public void OnCollideH(CollisionData data)
		{
			if (data.Hit != null && data.Hit.OnCollide != null)
			{
				data.Hit.OnCollide(data.Direction);
			}
			Speed.X = 0;
		}

		public void OnCollideV(CollisionData data)
		{
			if (data.Hit != null && data.Hit.OnCollide != null)
			{
				data.Hit.OnCollide(data.Direction);
			}
			Speed.Y = 0;
		}

		public void PlayAnim(string anim)
			=> Sprite.PlayOrWarn(anim);

		private void OnPlayer(Player player)
		{
			onCollide.Call(this, player);
		}

		public void SetSolidCollisionActive(bool active)
		{
			SolidCollidable = active;
		}

		public void SetCollisionActive(bool active)
		{
			Collidable = active;
		}

		public void SetEffectiveGravityMult(float mult)
		{
			GravityMult = mult;
		}
	}
}
