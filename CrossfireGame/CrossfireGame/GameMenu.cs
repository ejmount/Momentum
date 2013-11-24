using System;
using System.Collections.Generic;
using System.Linq;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CrossfireGame
{
	[MenuItem("Start Game", 1)]
	internal class GameMenu : GameState
	{
		private const float AmmoLimit = 20;
		private const float FiringDistance = 5;
		private const float FiringPower = 20;
		private const float gunSize = 3;
		private const float HeavyCost = 3;
		private const float LightCost = 1;
		private const int PHYSICS_ITERATIONS = 10;
		private const float Regeneration = 1 / 6f;
		private readonly TimeSpan FRAMEDURATION = TimeSpan.FromSeconds(1 / 60.0);

		private readonly Dictionary<PlayerIndex, string> playerBullets = new Dictionary<PlayerIndex, string>()
		{
			{PlayerIndex.One, "bullet_orange_hollow"},
			{PlayerIndex.Two, "bullet_purple_hollow"},
		};

		private Dictionary<PlayerIndex, float> AmmoStore = new Dictionary<PlayerIndex, float>();
		private List<Body> bullets = new List<Body>();
		private SoundEffect fireEffect;
		private SoundEffectInstance fireEffectInstance;
		private SoundEffect winEffect;
		private SoundEffectInstance winEffectInstance;
		private float horizScale;
		private SpriteFont MenuFont;
		private int physicssteps = 0;
		private List<ProgressBar> playerRateBars = new List<ProgressBar>();
		private List<Body> players = new List<Body>();
		private Body puck;
		private Random RNG = new Random();

		private Dictionary<PlayerIndex, int> Score = new Dictionary<PlayerIndex, int>
		{
			{PlayerIndex.One, 0},
			{PlayerIndex.Two, 0},
		};

		private World theWorld;
		private float verticalScale;
		private Texture2D white;
		private Vec2 worldBounds = new Vec2(100, 100);

		public GameMenu(Game1 g)
			: base(g)
		{
			verticalScale = parent.GraphicsDevice.Viewport.Height / worldBounds.Y;
			horizScale = parent.GraphicsDevice.Viewport.Width / worldBounds.X;

			theWorld = new World(new AABB() { LowerBound = new Vec2(0, 0), UpperBound = worldBounds }, Vec2.Zero, true);

			var puckdata = AbstractPhysics.CreateCircle(theWorld, 4, new Vec2(worldBounds.X / 2, worldBounds.Y / 2), Vec2.Zero, 0.1f, 0);
			puckdata.sprite = this.parent.GetContent<Texture2D>("Puck_50x50");
			puckdata.SetProperty("puck", true);
			puck = puckdata.thisBody;
			puck.SetMassFromShapes();

			// top edge
			var MB = AbstractPhysics.CreateBox(theWorld, worldBounds.X, 1, new Vec2(worldBounds.X / 2, 0), Vec2.Zero, density: 0);
			MB.color = Microsoft.Xna.Framework.Color.Red;
			MB.Cat = Category.World;

			// bottom edge
			MB = AbstractPhysics.CreateBox(theWorld, worldBounds.X, 1, new Vec2(worldBounds.X / 2, worldBounds.Y), Vec2.Zero, density: 0);
			MB.color = Microsoft.Xna.Framework.Color.Red;
			MB.Cat = Category.World;

			// left edge
			MB = AbstractPhysics.CreateBox(theWorld, 1, worldBounds.Y, new Vec2(0, worldBounds.Y / 2), Vec2.Zero, density: 0);
			MB.color = Microsoft.Xna.Framework.Color.Red;
			MB.Cat = Category.World;

			// right edge
			MB = AbstractPhysics.CreateBox(theWorld, 1, worldBounds.Y, new Vec2(worldBounds.X, worldBounds.Y / 2), Vec2.Zero, density: 0);
			MB.color = Microsoft.Xna.Framework.Color.Red;
			MB.Cat = Category.World;

			// left barrier.
			var leftBar = AbstractPhysics.CreateBox(theWorld, 0.1f, worldBounds.Y, new Vec2(3 + gunSize, worldBounds.Y / 2), Vec2.Zero, density: 0);
			leftBar.color = Microsoft.Xna.Framework.Color.Red;

			leftBar.SetProperty("polarization", new Vec2(1, 0));
			leftBar.SetProperty("edge", "left");

			// right barrier.
			var rightBar = AbstractPhysics.CreateBox(theWorld, 0.1f, worldBounds.Y, new Vec2(worldBounds.X - (3 + gunSize), worldBounds.Y / 2), Vec2.Zero, density: 0);
			rightBar.color = Microsoft.Xna.Framework.Color.Red;

			rightBar.SetProperty("polarization", new Vec2(-1, 0));
			rightBar.SetProperty("edge", "right");

			//theWorld.SetContactFilter(new OneWayFilter(barriers));
			theWorld.SetContactFilter(CollisionManager.getInstance());

			var player1 = AbstractPhysics.CreateBox(theWorld, gunSize, gunSize, new Vec2(3, worldBounds.Y / 2), Vec2.Zero, density: 1 / 9f, friction: 0).thisBody;
			(player1.GetUserData() as BodyMetadata).sprite = this.parent.GetContent<Texture2D>("P1_Gun_67x40");
			(player1.GetUserData() as BodyMetadata).Cat = Category.Transparent;
			players.Add(player1);

			ProgressBar p1bar = new ProgressBar(this.parent, new Rectangle(0, 0, 30, 10), ProgressBar.Orientation.HORIZONTAL_LR);
			p1bar.Initialize();

			p1bar.backgroundColor = Microsoft.Xna.Framework.Color.Black;
			p1bar.borderColorOuter = Microsoft.Xna.Framework.Color.Orange;
			p1bar.borderColorInner = Microsoft.Xna.Framework.Color.Black;
			p1bar.fillColor = Microsoft.Xna.Framework.Color.OrangeRed;
			p1bar.borderThicknessInner = 0;
			p1bar.borderThicknessOuter = 2;

			p1bar.minimum = 0;
			p1bar.maximum = AmmoLimit;
			playerRateBars.Add(p1bar);

			ProgressBar p2bar = new ProgressBar(this.parent, new Rectangle(0, 0, 30, 10), ProgressBar.Orientation.HORIZONTAL_LR);
			p2bar.Initialize();

			p2bar.backgroundColor = Microsoft.Xna.Framework.Color.Black;
			p2bar.borderColorOuter = Microsoft.Xna.Framework.Color.Purple;
			p2bar.borderColorInner = Microsoft.Xna.Framework.Color.Black;
			p2bar.fillColor = Microsoft.Xna.Framework.Color.MediumPurple;
			p2bar.borderThicknessInner = 0;
			p2bar.borderThicknessOuter = 2;

			p2bar.minimum = 0;
			p2bar.maximum = AmmoLimit;
			playerRateBars.Add(p2bar);

			var player2 = AbstractPhysics.CreateBox(theWorld, gunSize, gunSize, new Vec2(worldBounds.X - 3, worldBounds.Y / 2), Vec2.Zero, density: 1 / 9f, friction: 0).thisBody;
			(player2.GetUserData() as BodyMetadata).sprite = this.parent.GetContent<Texture2D>("P2_Gun_67x40");
			(player2.GetUserData() as BodyMetadata).Cat = Category.Transparent;
			players.Add(player2);
		}

		public override void Draw(GameTime time)
		{
			if (fireEffect == null)
			{
				fireEffect = this.parent.GetContent<SoundEffect>("fire");
				fireEffectInstance = fireEffect.CreateInstance();
			}
			if (winEffect == null)
			{
				winEffect = this.parent.GetContent<SoundEffect>("win");
				winEffectInstance = winEffect.CreateInstance();
			}

			if (white == null)
				white = this.parent.GetContent<Texture2D>("white");

			verticalScale = parent.GraphicsDevice.Viewport.Height / worldBounds.Y;
			horizScale = parent.GraphicsDevice.Viewport.Width / worldBounds.X;

			if (MenuFont == null)
				MenuFont = this.parent.Content.Load<SpriteFont>("defaultFont");
			// Lazily initialized because I couldn't get the initialization ordering with LoadContent right.

			parent.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
			SpriteBatch sb = new SpriteBatch(parent.GraphicsDevice);

			sb.Begin();

			// Code is duplicated because it's simpler than a loop and margin calculations.
			playerRateBars[0].SetPosition(5, intRound(players[0].GetPosition().Y * verticalScale) + 20);
			playerRateBars[0].value = AmmoStore.ContainsKey(PlayerIndex.One) ? AmmoStore[PlayerIndex.One] : 0;
			playerRateBars[0].Draw(sb);

			playerRateBars[1].SetPosition(intRound(worldBounds.Y * horizScale - 40), intRound(players[1].GetPosition().Y * verticalScale) + 20);
			playerRateBars[1].value = AmmoStore.ContainsKey(PlayerIndex.Two) ? AmmoStore[PlayerIndex.Two] : 0;
			playerRateBars[1].Draw(sb);

			var bodies = AbstractPhysics.GetBodies(theWorld);
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

			sb.DrawString(MenuFont, Score[PlayerIndex.One].ToString(), 10 * Vector2.UnitX, Microsoft.Xna.Framework.Color.Orange);

			var size = MenuFont.MeasureString(Score[PlayerIndex.Two].ToString());
			var textPos = new Vector2(parent.GraphicsDevice.Viewport.Width - size.X - 10, 0);

			sb.DrawString(MenuFont, Score[PlayerIndex.Two].ToString(), textPos, Microsoft.Xna.Framework.Color.MediumPurple);

			sb.End();
		}

		public override void Update(GameTime time)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (!AmmoStore.ContainsKey((PlayerIndex)i)) AmmoStore[(PlayerIndex)i] = 0;
				AmmoStore[(PlayerIndex)i] = System.Math.Min(AmmoStore[(PlayerIndex)i] + Regeneration, AmmoLimit);
			}

			CheckWinner();

			var garbage = bullets
				.Where(B => B.GetUserData() != null)
				.Where(B => time.TotalGameTime > (B.GetUserData() as BodyMetadata).expiry)
				.ToList();

			foreach (var g in garbage)
			{
				theWorld.DestroyBody(g);
				bullets.Remove(g);
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

		private void CheckWinner()
		{
			if (puck.GetUserData() is BodyMetadata)
			{
				var metapuck = (BodyMetadata)puck.GetUserData();
				if (metapuck.HasProperty("winner"))
				{
					if (metapuck.GetProperty<string>("winner").ToString() == "left")
						Score[PlayerIndex.Two]++;
					else if (metapuck.GetProperty<string>("winner").ToString() == "right")
						Score[PlayerIndex.One]++;

					metapuck.UnsetProperty("winner");

					foreach (var item in bullets)
					{
						var metadata = (BodyMetadata)item.GetUserData();
						metadata.expiry = TimeSpan.Zero;
					}

					foreach (var item in players)
					{
						item.SetPosition(new Vec2(item.GetPosition().X, worldBounds.Y / 2));
						item.SetLinearVelocity(Vec2.Zero);
					}

					puck.SetPosition(0.5f * worldBounds);
					puck.SetLinearVelocity(Vec2.Zero);

					winEffectInstance.Play();
				}
			}
		}

		private Controller Controller = Controller.GetController();

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

			if (input.HasFlag(Controller.Input.FireHeavy) || input.HasFlag(Controller.Input.FireLight))
			{
				var delta = new Vec2((float)System.Math.Cos(players[(int)player].GetAngle()), (float)System.Math.Sin(players[(int)player].GetAngle()));
				var nextPos = players[(int)player].GetPosition() + delta * FiringDistance;

				BodyMetadata entData;
				if (input.HasFlag(Controller.Input.FireHeavy) && AmmoStore[player] > HeavyCost)
				{
					entData = AbstractPhysics.CreateCircle(theWorld, 2, nextPos, delta * FiringPower, friction: 0);
					entData.expiry = time.TotalGameTime + new TimeSpan(hours: 0, minutes: 0, seconds: 8);
					entData.sprite = this.parent.GetContent<Texture2D>(playerBullets[player]);
					AmmoStore[player] -= HeavyCost;

					bullets.Add(entData.thisBody);

					AmmoStore[player] -= HeavyCost;
					fireEffectInstance.Play();
				}
				else if (input.HasFlag(Controller.Input.FireLight) && AmmoStore[player] > LightCost)
				{
					entData = AbstractPhysics.CreateCircle(theWorld, 1, nextPos, delta * FiringPower, friction: 0);
					entData.expiry = time.TotalGameTime + new TimeSpan(hours: 0, minutes: 0, seconds: 8);
					entData.sprite = this.parent.GetContent<Texture2D>(playerBullets[player]);
					AmmoStore[player] -= LightCost;

					bullets.Add(entData.thisBody);

					AmmoStore[player] -= LightCost;
					fireEffectInstance.Play();
				}
				else { }
			}
		}

		private int intRound(float d)
		{
			return (int)System.Math.Round(d);
		}
	}
}