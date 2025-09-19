using Celeste.Mod.BossesHelper.Code.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
	public abstract class BossActor : Actor
	{
		public readonly Sprite Sprite;

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

		private readonly float MaxFall;

		private float effectiveGravity;

		public BossActor(Vector2 position, string spriteName, Vector2 spriteScale, float maxFall,
			bool collidable, bool solidCollidable, float gravityMult)
			: base(position)
		{
			Collidable = collidable;
			SolidCollidable = solidCollidable;
			MaxFall = maxFall;
			GravityMult = gravityMult;
			if (GFX.SpriteBank.TryCreate(spriteName, out Sprite))
			{
				Sprite.Scale = spriteScale;
				Add(Sprite);
			}
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
				Speed.Y = Calc.Approach(Speed.Y, MaxFall, effectiveGravity * Engine.DeltaTime);
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
	}
}
