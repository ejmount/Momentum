using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CrossfireGame
{
	internal class RootMenu : GameState
	{
		private const float VERTICAL_MARGIN = 20;
		private int currentOption = 0;
		private SpriteFont MenuFont;
		private Texture2D bullet;
		private List<Tuple<MenuItem, Type>> Options = new List<Tuple<MenuItem, Type>>();
		private VerticalInput prevInput = VerticalInput.Neutral;

		public RootMenu(Game1 g)
			: base(g)
		{
			// ============================================================
			// Get all of the types which have a MenuItem attribute, and then sort them into the correct order
			// This is so the menu can be changed without editing any existing classes.
			// Open-closed FTW!
			// (Although perhaps excessively clever.)
			// ============================================================
			var MenuItemTypes = Assembly.GetExecutingAssembly().GetTypes()
				.Where(T => T.IsSubclassOf(typeof(GameState)))
				.Where(T => T.GetCustomAttributes(typeof(MenuItem), false).Length > 0);

			foreach (var Type in MenuItemTypes)
			{
				var attrib = (MenuItem)Type.GetCustomAttributes(typeof(MenuItem), false)[0];
				var optionName = attrib.Name;
				Options.Add(Tuple.Create(attrib, Type));
			}

			Options.Sort((T1, T2) =>
				{
					var firstOrder = T1.Item1.Order;
					var secondOrder = T2.Item1.Order;
					return firstOrder.CompareTo(secondOrder);
				});
			// ============================================================
			// ============================================================
		}
		private enum VerticalInput { Neutral, Up, Down }

		public override void Draw(GameTime time)
		{
			if (bullet == null)
				bullet = this.parent.Content.Load<Texture2D>("bullet_orange_filled");

			if (MenuFont == null)
				MenuFont = this.parent.Content.Load<SpriteFont>("defaultFont");
			// Lazily initialized because I couldn't get the initialization ordering with LoadContent right.

			parent.GraphicsDevice.Clear(Color.DarkBlue);
			SpriteBatch sb = new SpriteBatch(parent.GraphicsDevice);

			//float currentHeight = MENU_HEIGHT;

			float totalHeight = 0;

			foreach (var curOptionindex in Enumerate(Options))
			{
				var curOption = curOptionindex.Item2;
				var size = MenuFont.MeasureString(curOption.Item1.Name);
				totalHeight += size.Y + VERTICAL_MARGIN;
			}

			float currentHeight = this.parent.GraphicsDevice.Viewport.Height / 3 - totalHeight / 2;

			sb.Begin();
			foreach (var curOptionindex in Enumerate(Options))
			{
				var curOption = curOptionindex.Item2;
				var size = MenuFont.MeasureString(curOption.Item1.Name);
				var pos = new Vector2()
				{
					X = this.parent.GraphicsDevice.Viewport.Width / 2 - size.X / 2,
					Y = currentHeight
				};

				var col = Color.DarkGray;

				if ((curOptionindex.Item1 == currentOption))
				{
					col = Color.White;
					sb.Draw(bullet, new Rectangle(IR(pos.X - bullet.Width - size.Y / 2), IR(pos.Y), IR(size.Y), IR(size.Y)), Color.White);
				}
					sb.DrawString(MenuFont, curOption.Item1.Name, pos, col);
				currentHeight += size.Y + VERTICAL_MARGIN;
			}

			sb.End();
		}
		private int IR(float f)
		{
			return (int)Math.Round(f);
		}


		public override void Update(GameTime time)
		{
			VerticalInput curInput = VerticalInput.Neutral;

			if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Up)
				|| GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadUp)
				|| GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y > 0.5)
			{
				curInput = VerticalInput.Up;
			}
			else if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Down)
				|| GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadDown)
				|| GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < -0.5)
			{
				curInput = VerticalInput.Down;
			}

			if (curInput != prevInput)
			{
				if (curInput == VerticalInput.Up)
				{
					currentOption = Clamp(currentOption - 1, 0, Options.Count);
				}
				else if (curInput == VerticalInput.Down)
				{
					currentOption = Clamp(currentOption + 1, 0, Options.Count);
				}

				prevInput = curInput;
			}

			if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed
				|| Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Enter))
				this.parent.ChangeState(Options[currentOption].Item2);
		}

		private int Clamp(int v, int l, int u)
		{
			while (v >= u)
				v -= (u - l);
			while (v < l)
				v += (u - l);
			return v;
		}

		private IEnumerable<Tuple<int, T>> Enumerate<T>(IEnumerable<T> e)
		{
			int i = 0;
			foreach (var item in e)
			{
				yield return Tuple.Create(i++, item);
			}
		}
	}
}