using System;
using System.Collections.Generic;
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
		private const float gunSize = 3;

		private const float FiringDistance = 5;
		private const float FiringPower = 20;
		private const int PHYSICS_ITERATIONS = 10;
		private readonly TimeSpan FRAMEDURATION = TimeSpan.FromSeconds(1 / 60.0);
		private readonly Dictionary<PlayerIndex, string> playerBullets = new Dictionary<PlayerIndex, string>()
		{
			{PlayerIndex.One, "bullet_orange_hollow"},
			{PlayerIndex.Two, "bullet_purple_hollow"},
		};


		private Texture2D white;

		private World theWorld;

		private Vec2 worldBounds = new Vec2(100, 100);
		private Vec2 buffer = new Vec2(1, 1);

		private float verticalScale;
		private float horizScale;

		private List<Body> players = new List<Body>();
		private Dictionary<Body, Vec2> barriers = new Dictionary<Body, Vec2>();
		private List<Body> bullets = new List<Body>();
		private Dictionary<PlayerIndex, DateTime> lastFiredTimes = new Dictionary<PlayerIndex, DateTime>();


		private Body puck;

		public GameMenu(Game1 g)
			: base(g)
		{
			verticalScale = parent.GraphicsDevice.Viewport.Height / worldBounds.Y;
			horizScale = parent.GraphicsDevice.Viewport.Width / worldBounds.X;

			theWorld = new World(new AABB() { LowerBound = new Vec2(0, 0), UpperBound = worldBounds }, Vec2.Zero, true);

			var puckdata = AbstractPhysics.CreateCircle(theWorld, 4, new Vec2(worldBounds.X / 2, worldBounds.Y / 2), Vec2.Zero, 0.1f, 0);
			puckdata.sprite = this.parent.GetContent<Texture2D>("Puck_50x50");
			puck = puckdata.thisBody;
			puck.SetMassFromShapes();

			// top edge
			var MB = AbstractPhysics.CreateBox(theWorld, worldBounds.X, 1, new Vec2(worldBounds.X / 2, 0), Vec2.Zero, density: 0);
			MB.color = Microsoft.Xna.Framework.Color.Red;

			// bottom edge
			MB = AbstractPhysics.CreateBox(theWorld, worldBounds.X, 1, new Vec2(worldBounds.X / 2, worldBounds.Y), Vec2.Zero, density: 0);
			MB.color = Microsoft.Xna.Framework.Color.Red;

			// left edge
			MB = AbstractPhysics.CreateBox(theWorld, 1, worldBounds.Y, new Vec2(0, worldBounds.Y / 2), Vec2.Zero, density: 0);
			MB.color = Microsoft.Xna.Framework.Color.Red;

			// right edge
			MB = AbstractPhysics.CreateBox(theWorld, 1, worldBounds.Y, new Vec2(worldBounds.X, worldBounds.Y / 2), Vec2.Zero, density: 0);
			MB.color = Microsoft.Xna.Framework.Color.Red;

			// left barrier.
			var leftBar = AbstractPhysics.CreateBox(theWorld, 0.1f, worldBounds.Y, new Vec2(3 + gunSize, worldBounds.Y / 2), Vec2.Zero, density: 0);
			leftBar.color = Microsoft.Xna.Framework.Color.Red;

			barriers.Add(leftBar.thisBody, new Vec2(1, 0));

			// right barrier.
			var rightbar = AbstractPhysics.CreateBox(theWorld, 0.1f, worldBounds.Y, new Vec2(worldBounds.X - (3 + gunSize), worldBounds.Y / 2), Vec2.Zero, density: 0);
			rightbar.color = Microsoft.Xna.Framework.Color.Red;

			barriers.Add(rightbar.thisBody, new Vec2(-1, 0));

			theWorld.SetContactFilter(new OneWayFilter(barriers));

			var player1 = AbstractPhysics.CreateBox(theWorld, gunSize, gunSize, new Vec2(3, worldBounds.Y / 2), Vec2.Zero, density: 1 / 9f, friction: 0).thisBody;
			(player1.GetUserData() as BodyMetadata).sprite = this.parent.GetContent<Texture2D>("P1_Gun_67x40");
			players.Add(player1);

			var player2 = AbstractPhysics.CreateBox(theWorld, gunSize, gunSize, new Vec2(worldBounds.X - 3, worldBounds.Y / 2), Vec2.Zero, density: 1 / 9f, friction: 0).thisBody;
			(player2.GetUserData() as BodyMetadata).sprite = this.parent.GetContent<Texture2D>("P2_Gun_67x40");
			players.Add(player2);
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
							intRound((pos.X) * horizScale),
							intRound((pos.Y) * verticalScale),
							intRound((realExtents.X) * horizScale),
							intRound((realExtents.Y) * verticalScale)
							);

						Microsoft.Xna.Framework.Color col = Microsoft.Xna.Framework.Color.White;

						var sprite = white;

						if (B.GetUserData() is BodyMetadata && B.GetUserData() != null)
						{
							col = (B.GetUserData() as BodyMetadata).color;
							sprite = (B.GetUserData() as BodyMetadata).sprite ?? white;
						}

						sb.Draw(sprite, destRectangle, null, col, B.GetAngle(), new Vector2(sprite.Width / 2, sprite.Height / 2), SpriteEffects.None, 0);
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
			if (time.IsRunningSlowly && bullets.Count > 0)
			{
				var b = bullets[0];
				bullets.Remove(b);
				theWorld.DestroyBody(b);

			}

			var steps = (int)System.Math.Round(time.ElapsedGameTime.TotalMilliseconds / FRAMEDURATION.TotalMilliseconds);

			for (int i = 0; i < steps; i++)
			{
				theWorld.Step((float)FRAMEDURATION.TotalSeconds, PHYSICS_ITERATIONS, PHYSICS_ITERATIONS);
				physicssteps++;
			}

			int p = 0;
			foreach (var player in players)
			{
				var gunToPuck = puck.GetPosition() - players[p].GetPosition();

				float ang = (float)System.Math.Atan2(gunToPuck.Y, gunToPuck.X);
				players[p].SetAngle(ang);

				InterpretInput(time, (PlayerIndex)p);
				p++;
			}

			if (Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				this.parent.ChangeState(typeof(RootMenu));
			}
		}

		private void InterpretInput(GameTime time, PlayerIndex player)
		{
			var input = Controller.InterpretInput(player);

			if (input.HasFlag(Controller.Input.Up))
			{
				players[(int)player].ApplyImpulse(new Vec2(0, -1), players[(int)player].GetWorldCenter());
			}

			if (input.HasFlag(Controller.Input.Down))
			{
				players[(int)player].ApplyImpulse(new Vec2(0, 1), players[(int)player].GetWorldCenter());
			}
		/*	if (input.HasFlag(Controller.Input.Left))
			{
				players[(int)player].ApplyImpulse(new Vec2(-1, 0), players[(int)player].GetWorldCenter());
			}
			if (input.HasFlag(Controller.Input.Right))
			{
				players[(int)player].ApplyImpulse(new Vec2(1, 0), players[(int)player].GetWorldCenter());
			}*/

			if (!lastFiredTimes.ContainsKey(player) || (DateTime.Now - lastFiredTimes[player]).TotalSeconds > 0.07)
			{
				if (input.HasFlag(Controller.Input.FireHeavy))
				{
				}
				if (input.HasFlag(Controller.Input.FireLight))
				{
					var delta = new Vec2((float)System.Math.Cos(players[(int)player].GetAngle()), (float)System.Math.Sin(players[(int)player].GetAngle()));
					var nextPos = players[(int)player].GetPosition() + delta * FiringDistance;

					var metadata = AbstractPhysics.CreateCircle(theWorld, 1, nextPos, delta * FiringPower, friction: 0);
					metadata.expiry = time.TotalGameTime + new TimeSpan(hours: 0, minutes: 0, seconds: 10);
					metadata.sprite = this.parent.GetContent<Texture2D>(playerBullets[player]);

					bullets.Add(metadata.thisBody);

					lastFiredTimes[player] = DateTime.Now;
				}
			}
		}

		private int intRound(float d)
		{
			return (int)System.Math.Round(d);
		}
	}

	/// <summary>
	/// Takes a list of one-way bodies, and the vectors for the directions that objects ARE allowed to pass through them.
	/// </summary>
	internal class OneWayFilter : ContactFilter
	{
		private Dictionary<Body, Vec2> barrierTable;

		public OneWayFilter(Dictionary<Body, Vec2> barrierDirections)
		{
			barrierTable = barrierDirections;
		}

		public override bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
		{
			System.Diagnostics.Debug.Assert(fixtureA != null && fixtureB != null);
			Body barrier, other;

			if (!barrierTable.ContainsKey(fixtureA.Body) && !barrierTable.ContainsKey(fixtureB.Body))
				return base.ShouldCollide(fixtureA, fixtureB);

			if (barrierTable.ContainsKey(fixtureA.Body))
			{
				barrier = fixtureA.Body;
				other = fixtureB.Body;
			}
			else
			{
				other = fixtureA.Body;
				barrier = fixtureB.Body;
			}

			var direc = barrierTable[barrier];

			if (Vec2.Dot(other.GetLinearVelocity(), direc) > 0)
			{
				return false;
			}
			return base.ShouldCollide(fixtureA, fixtureB);
		}
	}
}