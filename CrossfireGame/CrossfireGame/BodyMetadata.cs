using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Box2DX.Dynamics;
using Microsoft.Xna.Framework;

namespace CrossfireGame
{
	public class BodyMetadata
	{
		public readonly Body thisBody;

		public Microsoft.Xna.Framework.Color Color = Microsoft.Xna.Framework.Color.White;

		public TimeSpan Expiry = TimeSpan.MaxValue;

		public BodyMetadata(Body B)
		{
			this.thisBody = B;
		}
	}
}
