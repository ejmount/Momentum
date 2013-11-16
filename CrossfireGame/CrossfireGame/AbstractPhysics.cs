using System.Collections.Generic;
using Box2DX.Dynamics;
using Box2DX.Common;
using Box2DX.Collision;

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

        static int n = 0;

		public static Body CreateEntity(World W, float width, float height, 
            Vec2 pos, Vec2 vel, 
            float mass = 1, float density = 1, float friction = 0.3f)
		{
			BodyDef bodydef = new BodyDef();
			bodydef.Position = pos;
			bodydef.LinearVelocity = vel;
			bodydef.MassData = new MassData() { Mass = mass };

			var B = W.CreateBody(bodydef);

			PolygonShape P = new PolygonShape();
			P.SetAsBox(width/2, height/2);

			FixtureDef fixtureDef = new PolygonDef() { Vertices = P.Vertices, VertexCount = P.VertexCount };
            fixtureDef.Density = density;
			fixtureDef.Friction = friction;
            fixtureDef.Restitution = 1f;

			B.CreateFixture(fixtureDef);

            B.SetUserData(n++);

			return B;

		}

         

	}
}