using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CrossfireGame
{
	internal class Controller
	{
		[Flags]
		public enum Input
		{
			None = 0,
			Up = 1 << 1,
			Down = 1 << 2,
			Left = 1 << 3,
			Right = 1 << 4,
			FireHeavy = 1 << 5,
			FireLight = 1 << 6,
		}

		public static Input InterpretInput(PlayerIndex P)
		{
			Input o = Input.None;

			if (Keyboard.GetState(P).IsKeyDown(Keys.Up)
				|| GamePad.GetState(P).ThumbSticks.Left.Y > .05
				|| GamePad.GetState(P).DPad.Up == ButtonState.Pressed)
			{
				o |= Input.Up;
			}
			else if (Keyboard.GetState(P).IsKeyDown(Keys.Down)
			|| GamePad.GetState(P).ThumbSticks.Left.Y < -.05
			|| GamePad.GetState(P).DPad.Down == ButtonState.Pressed)
			{
				o |= Input.Down;
			}

			if (Keyboard.GetState(P).IsKeyDown(Keys.Left)
				|| GamePad.GetState(P).ThumbSticks.Left.X < -.05
				|| GamePad.GetState(P).DPad.Left == ButtonState.Pressed)
			{
				o |= Input.Left;
			}
			else if (Keyboard.GetState(P).IsKeyDown(Keys.Right)
				|| GamePad.GetState(P).ThumbSticks.Left.X > .05
				|| GamePad.GetState(P).DPad.Right == ButtonState.Pressed)
			{
				o |= Input.Right;
			}

			if (GamePad.GetState(P).Triggers.Right > 0.5
				|| Keyboard.GetState(P).IsKeyDown(Keys.Z))
			{
				o |= Input.FireLight;
			}

			return o;
		}
	}
}