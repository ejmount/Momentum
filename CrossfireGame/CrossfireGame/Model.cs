using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace CrossFire.MVC
{
	public class Model
	{
		World theWorld;
		static Vec2 defaultGravity = new Vec2(0.0f, -9.8f);

		public Model(float width, float height)
			: this(width, height, defaultGravity)
		{

		}

		/// <summary>
		/// Create a new model
		/// </summary>
		/// <param name="width">Width in meters</param>
		/// <param name="height">Height in meters</param>
		/// <param name="gravity">Gravity vector. Defaults to (0, -9.8m/s²)</param>
		public Model(float width, float height, Vec2 gravity)
		{
			AABB bounds = new AABB() { LowerBound = new Vec2(0, 0), UpperBound = new Vec2(width, height) };
			theWorld = new World(bounds, gravity, true);
		}

		public Body CreateEntity(Vec2 pos, Vec2 vel )
		{

			BodyDef bodydef = new BodyDef();
			bodydef.Position = pos;
			bodydef.LinearVelocity = vel;
			bodydef.MassData = new MassData(){ Mass = 1 };
			var B = theWorld.CreateBody(bodydef);

			PolygonShape P = new PolygonShape();
			P.SetAsBox(0.05f, 0.05f);

			FixtureDef fixtureDef = new PolygonDef() { Vertices = P.Vertices, VertexCount = P.VertexCount };
			fixtureDef.Density = 1.0f;
			fixtureDef.Friction = 0.3f;

			B.CreateFixture(fixtureDef);
			return B;

		}

		public IEnumerable<Body> GetBodies()
		{
			return AbstractPhysics.GetBodies(theWorld);
		}


		public void Compute(float seconds, int iterations, float dx=1/60.0f)
		{
			if (iterations <= 0) throw new ArgumentException();

			for (int i = 0; i < seconds/dx; i++)
			{
				theWorld.Step(dx, iterations, iterations / 2);	
			}
		}

	}
}
