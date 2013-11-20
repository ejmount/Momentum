using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Box2DX.Dynamics;
using System.Reflection;
using Box2DX.Common;

namespace CrossfireGame
{
	class CollisionManager : ContactFilter
	{
		private static CollisionManager inst;
		private static List<MethodInfo> delegates = new List<MethodInfo>();

		static CollisionManager()
		{
			foreach (var klass in Assembly.GetExecutingAssembly().GetTypes())
			{
				foreach (var method in klass.GetMethods())
				{
					if (!method.IsStatic) continue;
					var param = method.GetParameters();
					if (param.Length == 2)// && method.ReturnType == typeof(UnsureBool))
					{
						if (param[0].ParameterType == typeof(Fixture)
							&& param[1].ParameterType == typeof(Fixture))
						{
							delegates.Add(method);
						}
					}
				}
			}

		}


		public static CollisionManager getInstance()
		{
			if (inst == null)
				inst = new CollisionManager();
			return inst;
		}

		public override bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
		{
			foreach (var item in delegates)
			{
				var res = (UnsureBool) item.Invoke(null, new object[] { fixtureA, fixtureB });
				if (res == UnsureBool.Yes)
					return true;
				else if (res == UnsureBool.No)
					return false;
			}
			return true;
		}

	}	

	enum UnsureBool { Dunno, Yes, No };

	[AttributeUsage(AttributeTargets.Method)]
	class CollisionMethod : Attribute
	{}

	class OriginalContactFilter
	{

		[CollisionMethod]
		public static UnsureBool CollideWalls(Fixture A, Fixture B)
		{
			if (!(A.Body.GetUserData() is BodyMetadata && B.Body.GetUserData() is BodyMetadata))
				return UnsureBool.Dunno;

			var metaA = (BodyMetadata)A.Body.GetUserData();
			var metaB = (BodyMetadata)B.Body.GetUserData();

			if (!metaA.HasProperty("polarization")
				&& !metaB.HasProperty("polarization"))
				return UnsureBool.Dunno;

			BodyMetadata barrier, other;

			if (metaA.HasProperty("polarization"))
			{
				barrier = metaA;
				other = metaB;
			}
			else
			{
				other = metaA;
				barrier = metaB;
			}

			var direc = barrier.GetProperty<Vec2>("polarization");

			if (Vec2.Dot(other.thisBody.GetLinearVelocity(), direc) > 0)
			{
				return UnsureBool.No;
			}

			return UnsureBool.Dunno;
		}

		[CollisionMethod]
		public static UnsureBool InvisibleThings(Fixture A, Fixture B)
		{
			if (!(A.Body.GetUserData() is BodyMetadata && B.Body.GetUserData() is BodyMetadata))
				return UnsureBool.Dunno;

			var metaA = (BodyMetadata)A.Body.GetUserData();
			var metaB = (BodyMetadata)B.Body.GetUserData();

			if (metaA.Cat == metaB.Cat)
				return UnsureBool.Dunno;

			if (metaA.Cat == Category.Normal && metaB.Cat == Category.Transparent
				|| metaB.Cat == Category.Normal && metaA.Cat == Category.Transparent)
				return UnsureBool.No;





			return UnsureBool.Dunno;
		}

	}


}
