using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CrossfireGame
{
	public class BodyMetadata
	{
		public readonly Box2DX.Dynamics.Body thisBody;

		public Color color = Color.White;
		public Texture2D sprite;
		public TimeSpan expiry = TimeSpan.MaxValue;

		public BodyMetadata(Box2DX.Dynamics.Body B)
		{
			this.thisBody = B;
		}
	}
}