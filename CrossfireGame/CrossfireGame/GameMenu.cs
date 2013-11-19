using System;
using System.Linq;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CrossfireGame
{
	[MenuItem("Start Game", 1)]
	internal class GameMenu : GameState
	{
		private const int PHYSICS_ITERATIONS = 10;
		private readonly TimeSpan FRAMEDURATION = TimeSpan.FromSeconds(1 / 60.0);

		private Texture2D white;

		private World theWorld;

		private Vec2 worldBounds = new Vec2(100, 100);
		private Vec2 buffer = new Vec2(1, 1);

		private float verticalScale;
		private float horizScale;

		private Body player1;

		public GameMenu(Game1 g)
			: base(g)
		{
			verticalScale = parent.GraphicsDevice.Viewport.Height / worldBounds.Y;
			horizScale = parent.GraphicsDevice.Viewport.Width / worldBounds.X;

			theWorld = new World(new AABB() { LowerBound = new Vec2(0, 0), UpperBound = worldBounds }, Vec2.Zero, true);

			player1 = AbstractPhysics.CreateEntity(theWorld, 3f, 3f, new Vec2(5, 3), Vec2.Zero, friction: 0)
					.thisBody;

			(player1.GetUserData() as BodyMetadata).color = Microsoft.Xna.Framework.Color.Blue;

			// top edge
			var MB = AbstractPhysics.CreateEntity(theWorld, worldBounds.X, 1, new Vec2(worldBounds.X / 2, 0), Vec2.Zero, mass: 0, density: 0);
			MB.color = Microsoft.Xna.Framework.Color.Red;

			// bottom edge
			MB = AbstractPhysics.CreateEntity(theWorld, worldBounds.X, 1, new Vec2(worldBounds.X / 2, worldBounds.Y), Vec2.Zero, mass: 0, density: 0);
			MB.color = Microsoft.Xna.Framework.Color.Red;

			// left edge
			MB = AbstractPhysics.CreateEntity(theWorld, 1, worldBounds.Y, new Vec2(0, worldBounds.Y / 2), Vec2.Zero, mass: 0, density: 0);
			MB.color = Microsoft.Xna.Framework.Color.Red;

			// right edge
			MB = AbstractPhysics.CreateEntity(theWorld, 1, worldBounds.Y, new Vec2(worldBounds.X, worldBounds.Y / 2), Vec2.Zero, mass: 0, density: 0);
			MB.color = Microsoft.Xna.Framework.Color.Red;

			


		}

		private SpriteFont MenuFont;

		public override void Draw(GameTime time)
		{
			
			if (white == null)
				white = this.parent.GetContent<Texture2D>("white");

			verticalScale = parent.GraphicsDevice.Viewport.Height / worldBounds.Y;
			horizScale = parent.GraphicsDevice.Viewport.Width / worldBounds.X;

			parent.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
			SpriteBatch sb = new SpriteBatch(parent.GraphicsDevice);

			var bodies = AbstractPhysics.GetBodies(theWorld);

			if (MenuFont == null)
				MenuFont = this.parent.Content.Load<SpriteFont>("defaultFont");
			// Lazily initialized because I couldn't get the initialization ordering with LoadContent right.

			sb.Begin();
			foreach (var B in bodies)
			{
				var pos = B.GetPosition();
				var fixes = AbstractPhysics.GetFixtures(B);
				if (B.GetUserData() != null)
				{
					if (fixes.Any())
					{
						var firstFix = fixes.First();

						AABB box = new AABB();
						firstFix.Shape.ComputeAABB(out box, XForm.Identity);

						var realExtents = box.UpperBound - box.LowerBound;

						var destRectangle = new Rectangle(
							intRound((pos.X + box.LowerBound.X) * horizScale),
							intRound((pos.Y + box.LowerBound.Y) * verticalScale),
							intRound((realExtents.X) * horizScale),
							intRound((realExtents.Y) * verticalScale)
							);

						Microsoft.Xna.Framework.Color col = Microsoft.Xna.Framework.Color.White;

						if (B.GetUserData() is BodyMetadata && B.GetUserData() != null)
						{
							col = (B.GetUserData() as BodyMetadata).color;
						}

						/*if (B.GetUserData() is Microsoft.Xna.Framework.Color && B.GetUserData() != null)
							col = (Microsoft.Xna.Framework.Color)B.GetUserData();*/

						sb.Draw(white, destRectangle, col);
					}
				}
			}

			var garbage = bodies
				.Where(B => B.GetUserData() != null)
				.Where(B => time.TotalGameTime > (B.GetUserData() as BodyMetadata).expiry)
				.ToList();

			foreach (var g in garbage)
			{
				theWorld.DestroyBody(g);
			}


			sb.DrawString(MenuFont, time.TotalGameTime.TotalMilliseconds.ToString(), Microsoft.Xna.Framework.Vector2.Zero, Microsoft.Xna.Framework.Color.White);

			var size = MenuFont.MeasureString((time.TotalGameTime.TotalMilliseconds / FRAMEDURATION.TotalMilliseconds).ToString());
			var textPos = new Vector2(0, parent.GraphicsDevice.Viewport.Height - size.Y);

			sb.DrawString(MenuFont, (time.TotalGameTime.TotalMilliseconds / FRAMEDURATION.TotalMilliseconds).ToString() + "," + physicssteps + "," + Controller.InterpretInput(PlayerIndex.One).ToString(), textPos, Microsoft.Xna.Framework.Color.White);

			sb.End();
		}

		private DateTime lastspawn;
		private Random RNG = new Random();
		private int physicssteps = 0;

		public override void Update(GameTime time)
		{
			var steps = (int)System.Math.Round(time.ElapsedGameTime.TotalMilliseconds / FRAMEDURATION.TotalMilliseconds);

			for (int i = 0; i < steps; i++)
			{
				theWorld.Step((float)FRAMEDURATION.TotalSeconds, PHYSICS_ITERATIONS, PHYSICS_ITERATIONS);
				physicssteps++;
			}

			var input = Controller.InterpretInput(PlayerIndex.One);

			if (input.HasFlag(Controller.Input.Up))
			{
				player1.ApplyImpulse(new Vec2(0, -1), player1.GetWorldCenter());
			}

			if (input.HasFlag(Controller.Input.Down))
			{
				player1.ApplyImpulse(new Vec2(0, 1), player1.GetWorldCenter());
			}
			if (input.HasFlag(Controller.Input.Left))
			{
				player1.ApplyImpulse(new Vec2(-1, 0), player1.GetWorldCenter());
			}
			if (input.HasFlag(Controller.Input.Right))
			{
				player1.ApplyImpulse(new Vec2(1, 0), player1.GetWorldCenter());
			}
			if (input.HasFlag(Controller.Input.FireHeavy))
			{
			}
			if (input.HasFlag(Controller.Input.FireLight))
			{
			}

			if (Controller.InterpretInput(PlayerIndex.One).HasFlag(Controller.Input.FireLight)
				&& (DateTime.Now - lastspawn).TotalSeconds > 0.07)
			{
				var metadata = AbstractPhysics.CreateEntity(theWorld, 1, 1, player1.GetPosition() + new Vec2(10, 0), new Vec2(10, 0), friction: 0);
				metadata.expiry = time.TotalGameTime + new TimeSpan(hours: 0, minutes: 1, seconds:0);

				lastspawn = DateTime.Now;
			}

			if (GamePad.GetState(0).Buttons.A == ButtonState.Pressed)
			{
				this.parent.ChangeState(typeof(RootMenu));
			}
		}

		private int intRound(float d)
		{
			return (int)System.Math.Round(d);
		}
	}
}