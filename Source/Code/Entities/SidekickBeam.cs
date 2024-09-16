﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Pooled]
    [Tracked(false)]
    internal class SidekickBeam : Entity
    {
        private SidekickTarget Target;

        public static ParticleType P_Dissipate;

        public const float ChargeTime = 1.4f;

        public const float FollowTime = 0.9f;

        public const float ActiveTime = 0.12f;

        private const float AngleStartOffset = 100f;

        private const float RotationSpeed = 200f;

        private const float CollideCheckSep = 2f;

        private const float BeamLength = 2000f;

        private const float BeamStartDist = 12f;

        private const int BeamsDrawn = 15;

        private const float SideDarknessAlpha = 0.35f;

        private BadelineSidekick sidekick;

        private Player player;

        private Sprite beamSprite;

        private Sprite beamStartSprite;

        private float chargeTimer;

        private float followTimer;

        private float activeTimer;

        private float angle;

        private float beamAlpha;

        private float sideFadeAlpha;

        private VertexPositionColor[] fade = new VertexPositionColor[24];

        public SidekickBeam()
        {
            Add(beamSprite = GFX.SpriteBank.Create("badeline_beam"));
            beamSprite.OnLastFrame = (string anim) =>
            {
                if (anim == "shoot")
                {
                    RemoveSelf();
                }
            };
            Add(beamStartSprite = GFX.SpriteBank.Create("badeline_beam_start"));
            beamStartSprite.Visible = false;
            base.Depth = -1000000;
        }

        public SidekickBeam Init(BadelineSidekick sidekick, Player target)
        {
            this.sidekick = sidekick;
            chargeTimer = 1.4f;
            followTimer = 0.9f;
            activeTimer = 0.12f;
            beamSprite.Play("charge");
            sideFadeAlpha = 0f;
            beamAlpha = 0f;
            int num = ((target.Y <= sidekick.Y + 16f) ? 1 : (-1));
            if (target.X >= sidekick.X)
            {
                num *= -1;
            }
            angle = Calc.Angle(sidekick.BeamOrigin, target.Center);
            Vector2 to = Calc.ClosestPointOnLine(sidekick.BeamOrigin, sidekick.BeamOrigin + Calc.AngleToVector(angle, 2000f), target.Center);
            to += (target.Center - sidekick.BeamOrigin).Perpendicular().SafeNormalize(100f) * num;
            angle = Calc.Angle(sidekick.BeamOrigin, to);
            return this;
        }

        public override void Update()
        {
            base.Update();
            Level level = SceneAs<Level>();
            Target = level.Tracker.GetNearestEntity<SidekickTarget>(Center);
            beamAlpha = Calc.Approach(beamAlpha, 1f, 2f * Engine.DeltaTime);
            if (chargeTimer > 0f)
            {
                sideFadeAlpha = Calc.Approach(sideFadeAlpha, 1f, Engine.DeltaTime);
                if (Target != null)
                {
                    followTimer -= Engine.DeltaTime;
                    chargeTimer -= Engine.DeltaTime;
                    if (followTimer > 0f && Target.Center != sidekick.BeamOrigin)
                    {
                        Vector2 val = Calc.ClosestPointOnLine(sidekick.BeamOrigin, sidekick.BeamOrigin + Calc.AngleToVector(angle, 2000f), Target.Center);
                        Vector2 center = Target.Center;
                        val = Calc.Approach(val, center, 200f * Engine.DeltaTime);
                        angle = Calc.Angle(sidekick.BeamOrigin, val);
                    }
                    else if (beamSprite.CurrentAnimationID == "charge")
                    {
                        beamSprite.Play("lock");
                    }
                    if (chargeTimer <= 0f)
                    {
                        SceneAs<Level>().DirectionalShake(Calc.AngleToVector(angle, 1f), 0.15f);
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        DissipateParticles();
                    }
                }
            }
            else if (activeTimer > 0f)
            {
                sideFadeAlpha = Calc.Approach(sideFadeAlpha, 0f, Engine.DeltaTime * 8f);
                if (beamSprite.CurrentAnimationID != "shoot")
                {
                    beamSprite.Play("shoot");
                    beamStartSprite.Play("shoot", restart: true);
                }
                activeTimer -= Engine.DeltaTime;
                if (activeTimer > 0f)
                {
                    TargetCollideCheck();
                }
            }
        }

        private void DissipateParticles()
        {
            Level level = SceneAs<Level>();
            Vector2 vector = level.Camera.Position + new Vector2(160f, 90f);
            Vector2 vector2 = sidekick.BeamOrigin + Calc.AngleToVector(angle, 12f);
            Vector2 vector3 = sidekick.BeamOrigin + Calc.AngleToVector(angle, 2000f);
            Vector2 vector4 = (vector3 - vector2).Perpendicular().SafeNormalize();
            Vector2 vector5 = (vector3 - vector2).SafeNormalize();
            Vector2 min = -vector4 * 1f;
            Vector2 max = vector4 * 1f;
            float direction = vector4.Angle();
            float direction2 = (-vector4).Angle();
            float num = Vector2.Distance(vector, vector2) - 12f;
            vector = Calc.ClosestPointOnLine(vector2, vector3, vector);
            for (int i = 0; i < 200; i += 12)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    level.ParticlesFG.Emit(P_Dissipate, vector + vector5 * i + vector4 * 2f * j + Calc.Random.Range(min, max), direction);
                    level.ParticlesFG.Emit(P_Dissipate, vector + vector5 * i - vector4 * 2f * j + Calc.Random.Range(min, max), direction2);
                    if (i != 0 && (float)i < num)
                    {
                        level.ParticlesFG.Emit(P_Dissipate, vector - vector5 * i + vector4 * 2f * j + Calc.Random.Range(min, max), direction);
                        level.ParticlesFG.Emit(P_Dissipate, vector - vector5 * i - vector4 * 2f * j + Calc.Random.Range(min, max), direction2);
                    }
                }
            }
        }

        private void TargetCollideCheck()
        {
            Vector2 vector = sidekick.BeamOrigin + Calc.AngleToVector(angle, 12f);
            Vector2 vector2 = sidekick.BeamOrigin + Calc.AngleToVector(angle, 2000f);
            Vector2 vector3 = (vector2 - vector).Perpendicular().SafeNormalize(2f);
            SidekickTarget target = base.Scene.CollideFirst<SidekickTarget>(vector + vector3, vector2 + vector3);
            if (target == null)
            {
                target = base.Scene.CollideFirst<SidekickTarget>(vector - vector3, vector2 - vector3);
            }
            if (target == null)
            {
                target = base.Scene.CollideFirst<SidekickTarget>(vector, vector2);
            }
            target?.OnLaser();
        }

        public override void Render()
        {
            Vector2 beamOrigin = sidekick.BeamOrigin;
            Vector2 vector = Calc.AngleToVector(angle, beamSprite.Width);
            beamSprite.Rotation = angle;
            beamSprite.Color = Color.White * beamAlpha;
            beamStartSprite.Rotation = angle;
            beamStartSprite.Color = Color.White * beamAlpha;
            if (beamSprite.CurrentAnimationID == "shoot")
            {
                beamOrigin += Calc.AngleToVector(angle, 8f);
            }
            for (int i = 0; i < 15; i++)
            {
                beamSprite.RenderPosition = beamOrigin;
                beamSprite.Render();
                beamOrigin += vector;
            }
            if (beamSprite.CurrentAnimationID == "shoot")
            {
                beamStartSprite.RenderPosition = sidekick.BeamOrigin;
                beamStartSprite.Render();
            }
            GameplayRenderer.End();
            Vector2 vector2 = vector.SafeNormalize();
            Vector2 vector3 = vector2.Perpendicular();
            Color color = Color.Black * sideFadeAlpha * 0.35f;
            Color transparent = Color.Transparent;
            vector2 *= 4000f;
            vector3 *= 120f;
            int v = 0;
            Quad(ref v, beamOrigin, -vector2 + vector3 * 2f, vector2 + vector3 * 2f, vector2 + vector3, -vector2 + vector3, color, color);
            Quad(ref v, beamOrigin, -vector2 + vector3, vector2 + vector3, vector2, -vector2, color, transparent);
            Quad(ref v, beamOrigin, -vector2, vector2, vector2 - vector3, -vector2 - vector3, transparent, color);
            Quad(ref v, beamOrigin, -vector2 - vector3, vector2 - vector3, vector2 - vector3 * 2f, -vector2 - vector3 * 2f, color, color);
            GFX.DrawVertices((base.Scene as Level).Camera.Matrix, fade, fade.Length);
            GameplayRenderer.Begin();
        }

        private void Quad(ref int v, Vector2 offset, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color ab, Color cd)
        {
            fade[v].Position.X = offset.X + a.X;
            fade[v].Position.Y = offset.Y + a.Y;
            fade[v++].Color = ab;
            fade[v].Position.X = offset.X + b.X;
            fade[v].Position.Y = offset.Y + b.Y;
            fade[v++].Color = ab;
            fade[v].Position.X = offset.X + c.X;
            fade[v].Position.Y = offset.Y + c.Y;
            fade[v++].Color = cd;
            fade[v].Position.X = offset.X + a.X;
            fade[v].Position.Y = offset.Y + a.Y;
            fade[v++].Color = ab;
            fade[v].Position.X = offset.X + c.X;
            fade[v].Position.Y = offset.Y + c.Y;
            fade[v++].Color = cd;
            fade[v].Position.X = offset.X + d.X;
            fade[v].Position.Y = offset.Y + d.Y;
            fade[v++].Color = cd;
        }
    }
}
