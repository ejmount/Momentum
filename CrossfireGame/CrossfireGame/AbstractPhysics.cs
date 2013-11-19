using System.Collections.Generic;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace CrossfireGame
{
	/// <summary>
	/// A facade layer between the model and the full Box2DX library, providing helper functions.
	/// </summary>
	public static class AbstractPhysics
	{
		/// <summary>
		/// Produces an Enumerable for all of the bodies in a given World.
		/// </summary>
		/// <param name="W"></param>
		/// <returns></returns>
		public static IEnumerable<Body> GetBodies(World W)
		{
			var b = W.GetBodyList();
			do
			{
				if (b != null)
				{
					yield return b;
					b = b.GetNext();
				}
			}
			while (b != null);
		}

		public static IEnumerable<Fixture> GetFixtures(Body B)
		{
			var J = B.GetFixtureList();
			do
			{
				if (J != null)
				{
					yield return J;
					J = J.Next;
				}
			}
			while (J != null);
		}

		private static Body CreateEntity(World W, Vec2 pos, Vec2 vel)
		{
			BodyDef bodydef = new BodyDef();
			bodydef.Position = pos;
			bodydef.LinearVelocity = vel;

			var B = W.CreateBody(bodydef);

			return B;
		}

		public static BodyMetadata CreateBox(World W, float width, float height, Vec2 pos, Vec2 vel, float density = 1, float friction = 0)
		{
			var B = CreateEntity(W, pos, vel);
			B.SetMass(new MassData() { Mass = width * height * density });

			PolygonShape P = new PolygonShape();
			P.SetAsBox(width / 2, height / 2);

			FixtureDef fixtureDef = new PolygonDef() { Vertices = P.Vertices, VertexCount = P.VertexCount };
			fixtureDef.Density = density;
			fixtureDef.Friction = friction;
			fixtureDef.Restitution = 1f;

			B.CreateFixture(fixtureDef);

			var BM = new BodyMetadata(B);

			B.SetUserData(BM);

			return BM;
		}

		public static BodyMetadata CreateCircle(World W, float radius, Vec2 pos, Vec2 vel, float density = 1, float friction = 0)
		{
			var B = CreateEntity(W, pos, vel);

			B.SetMass(new MassData() { Mass = Settings.Pi * radius * radius * density });

			FixtureDef fixtureDef = new CircleDef() { Radius = radius };
			fixtureDef.Density = density;
			fixtureDef.Friction = friction;
			fixtureDef.Restitution = 1f;

			B.CreateFixture(fixtureDef);

			var BM = new BodyMetadata(B);

			B.SetUserData(BM);

			return BM;
		}
	}
}