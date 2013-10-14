using System.Collections.Generic;
using Box2DX.Dynamics;

namespace CrossFire
{
	/// <summary>
	/// A facade layer between the model and the full Box2DX library, providing helper functions. 
	/// </summary>
	public class AbstractPhysics
	{
		private class BodyEnumerable : IEnumerable<Body>
		{
			protected World _W;
			protected BodyEnumerator e;

			public BodyEnumerable(World W)
			{
				_W = W;
				e = new BodyEnumerator(W);
			}

			IEnumerator<Body> IEnumerable<Body>.GetEnumerator()
			{
				return e;
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return e;
			}
		}

		private class BodyEnumerator : IEnumerator<Body>
		{
			protected World _W;
			protected Body head;
			protected Body cur;

			public BodyEnumerator(World W)
			{
				_W = W;
				head = _W.GetBodyList();
			}

			public Body Current
			{
				get { return cur; }
			}

			public void Dispose()
			{
			}

			object System.Collections.IEnumerator.Current
			{
				get { return (object)cur; }
			}

			public bool MoveNext()
			{
				if (cur == null)
					cur = head;
				cur = cur.GetNext();
				return (cur != null);
			}

			public void Reset()
			{
				cur = null;
			}
		}

		public static IEnumerable<Body> GetBodies(World W)
		{
			return new BodyEnumerable(W);
		}
	}
}