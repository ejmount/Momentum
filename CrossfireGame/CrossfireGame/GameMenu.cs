﻿using System;
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

        Vec2 worldBounds = new Vec2(100, 100);
        Vec2 buffer = new Vec2(1, 1);


        float verticalScale;
        float horizScale;

        Body puck;


		public GameMenu(Game1 g)
			: base(g)
		{

            verticalScale = parent.GraphicsDevice.Viewport.Height / worldBounds.Y;
            horizScale = parent.GraphicsDevice.Viewport.Width / worldBounds.X;


            theWorld = new World(new AABB() { LowerBound = new Vec2(0, 0), UpperBound = worldBounds }, Vec2.Zero, true);
			puck = AbstractPhysics.CreateEntity(theWorld, 1f, 1f, new Vec2(5, 3), new Vec2(7, 0), friction: 0);
			//AbstractPhysics.CreateEntity(theWorld, 1f, 1f, new Vec2(10,3), new Vec2(-5, 0), friction:0);

            puck.SetUserData(Microsoft.Xna.Framework.Color.Blue);

            // top edge
            AbstractPhysics.CreateEntity(theWorld, worldBounds.X, 1, new Vec2(worldBounds.X/2, 0), Vec2.Zero, mass:0, density:0);

            // bottom edge
            AbstractPhysics.CreateEntity(theWorld, worldBounds.X, 1, new Vec2(worldBounds.X/2, worldBounds.Y), Vec2.Zero, mass: 0, density: 0);
                        
            // left edge
            AbstractPhysics.CreateEntity(theWorld, 1, worldBounds.Y, new Vec2(0, worldBounds.Y/2), Vec2.Zero, mass: 0, density: 0);

            // right edge
            AbstractPhysics.CreateEntity(theWorld, 1, worldBounds.Y, new Vec2(worldBounds.X, worldBounds.Y/2), Vec2.Zero, mass: 0, density: 0);

		}

      



        SpriteFont MenuFont;

		public override void Draw(GameTime time)
		{
			if (white == null)
				white = this.parent.Content.Load<Texture2D>("white");

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
                    if (B.GetUserData() is Microsoft.Xna.Framework.Color && B.GetUserData() != null)
                        col = (Microsoft.Xna.Framework.Color)B.GetUserData();

					sb.Draw(white, destRectangle, col);

				}
            } System.Diagnostics.Debug.WriteLine("");

            sb.DrawString(MenuFont, time.TotalGameTime.TotalMilliseconds.ToString(), Microsoft.Xna.Framework.Vector2.Zero, Microsoft.Xna.Framework.Color.White);

            var size = MenuFont.MeasureString((time.TotalGameTime.TotalMilliseconds / FRAMEDURATION.TotalMilliseconds).ToString());
            var textPos = new Vector2(0, parent.GraphicsDevice.Viewport.Height - size.Y);


            sb.DrawString(MenuFont, (time.TotalGameTime.TotalMilliseconds / FRAMEDURATION.TotalMilliseconds).ToString() + "," + physicssteps, textPos, Microsoft.Xna.Framework.Color.White);
            




			sb.End();
		}

        DateTime lastspawn;
        Random RNG = new Random();
        int physicssteps = 0;

		public override void Update(GameTime time)
		{
			var steps = (int) System.Math.Round(time.ElapsedGameTime.TotalMilliseconds / FRAMEDURATION.TotalMilliseconds);

			for (int i = 0; i < steps; i++)
			{
				theWorld.Step((float) FRAMEDURATION.TotalSeconds, PHYSICS_ITERATIONS, PHYSICS_ITERATIONS);
                physicssteps++;
			}

            if (Keyboard.GetState(0).IsKeyDown(Keys.Right) && AbstractPhysics.GetBodies(theWorld).Any( B => B.IsDynamic()))
            {
                var body = puck;
                body.ApplyImpulse(new Vec2(1, 0), body.GetWorldCenter());
            }

            if (Keyboard.GetState(0).IsKeyDown(Keys.Left) && AbstractPhysics.GetBodies(theWorld).Any(B => B.IsDynamic()))
            {
                var body = puck;
                body.ApplyImpulse(new Vec2(-1, 0), body.GetWorldCenter());
            }

            if (Keyboard.GetState(0).IsKeyDown(Keys.Down) && AbstractPhysics.GetBodies(theWorld).Any(B => B.IsDynamic()))
            {
                var body = puck;
                body.ApplyImpulse(new Vec2(0, 1), body.GetWorldCenter());
            }


            if (Keyboard.GetState(0).IsKeyDown(Keys.Up) && AbstractPhysics.GetBodies(theWorld).Any(B => B.IsDynamic()))
            {
                var body = puck;
                body.ApplyImpulse(new Vec2(0, -1), body.GetWorldCenter());
            }

            if (Keyboard.GetState(0).IsKeyDown(Keys.V) && (DateTime.Now - lastspawn).TotalSeconds > 0.05)
            {
                float x = (float) RNG.NextDouble();
                float y = (float) System.Math.Sqrt(1-x*x);
                AbstractPhysics.CreateEntity(theWorld, 1, 1, puck.GetPosition()-((float)FRAMEDURATION.TotalSeconds*puck.GetLinearVelocity()), new Vec2(x, y), friction: 0);
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
