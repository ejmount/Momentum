using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Box2DX.Common;
using Box2DX.Dynamics;
using Box2DX.Collision;

namespace CrossfireGame
{
	[MenuItem("Start Game",1)]
	class GameMenu : GameState
	{
		const int PHYSICS_ITERATIONS = 5;
		readonly TimeSpan FRAMEDURATION = TimeSpan.FromSeconds(1 / 60.0);

		Texture2D white;

		World theWorld;

		public GameMenu(Game1 g)
			: base(g)
		{
			theWorld = new World(new AABB() { LowerBound = new Vec2(0, 0), UpperBound = new Vec2(10, 10) }, Vec2.Zero, true);
			AbstractPhysics.CreateEntity(theWorld, 1f, 1f, Vec2.Zero, new Vec2(1, 0));
			AbstractPhysics.CreateEntity(theWorld, 1f, 1f, new Vec2(5,0), new Vec2(-1, 0));
		}

		public override void Draw(GameTime time)
		{
			if (white == null)
				white = this.parent.Content.Load<Texture2D>("white");


			parent.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
			SpriteBatch sb = new SpriteBatch(parent.GraphicsDevice);

			var bodies = AbstractPhysics.GetBodies(theWorld);


			sb.Begin();
			foreach (var B in bodies)
			{
				var pos = B.GetPosition();
				var fixes = AbstractPhysics.GetFixtures(B);
				if (fixes.Any())
				{
					var firstFix = fixes.First();

					AABB box = new AABB();
					firstFix.Shape.ComputeAABB(out box, XForm.Identity);

					var destRectangle = new Rectangle((int)pos.X, (int)pos.Y, (int)box.Extents.X, (int)box.Extents.Y);
					sb.Draw(white, destRectangle, Microsoft.Xna.Framework.Color.White);
				}
			}
			sb.End();
		}

		public override void Update(GameTime time)
		{
			var steps = (int) System.Math.Round(time.ElapsedGameTime.TotalMilliseconds / FRAMEDURATION.TotalMilliseconds);

			for (int i = 0; i < steps; i++)
			{
				theWorld.Step(1 / 60.0f, PHYSICS_ITERATIONS, PHYSICS_ITERATIONS);	
			}


			if (GamePad.GetState(0).Buttons.A == ButtonState.Pressed)
			{
				this.parent.ChangeState(typeof(RootMenu));
			}
			
		}
	}
}
