using System.Collections.Generic;
using Box2DX.Dynamics;

namespace CrossFire
{
	/// <summary>
	/// A facade layer between the model and the full Box2DX library, providing helper functions. 
	/// </summary>
	public class AbstractPhysics
	{
		/// <summary>
		/// Produces an Enumerable for all of the bodies in a given World. 
		/// </summary>
		/// <param name="W"></param>
		/// <returns></returns>
		public static IEnumerable<Body> GetBodies(World W)
		{
			//return new BodyEnumerable(W);

			var b = W.GetBodyList();
			do
			{
				yield return b;
				b = b.GetNext();
			}
			while (b != null);

		}
	}
}