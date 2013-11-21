using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

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

		public static readonly Dictionary<PlayerIndex, Dictionary<Input, Keys>> keymapping =
			new Dictionary<PlayerIndex, Dictionary<Input, Keys>>()
			{
				{
					PlayerIndex.One, 
					new Dictionary<Input, Keys>(){
						{Input.Up, Keys.W},
						{Input.Down, Keys.S},
						{Input.Left, Keys.A},
						{Input.Right, Keys.D},
						{Input.FireLight, Keys.Q},
						{Input.FireHeavy, Keys.E}
					}
				},
				{
					PlayerIndex.Two, 
					new Dictionary<Input, Keys>(){
						{Input.Up, Keys.Up},
						{Input.Down, Keys.Down},
						{Input.Left, Keys.Left},
						{Input.Right, Keys.Right},
						{Input.FireLight, Keys.RightControl},
						{Input.FireHeavy, Keys.RightShift}
					}
				},
			};

		public static Input InterpretInput(PlayerIndex P)
		{
			Input o = Input.None;

			if (Keyboard.GetState(P).IsKeyDown(keymapping[P][Input.Up])
				|| GamePad.GetState(P).ThumbSticks.Left.Y > .05
				|| GamePad.GetState(P).DPad.Up == ButtonState.Pressed)
			{
				o |= Input.Up;
			}
			else if (Keyboard.GetState(P).IsKeyDown(keymapping[P][Input.Down])
			|| GamePad.GetState(P).ThumbSticks.Left.Y < -.05
			|| GamePad.GetState(P).DPad.Down == ButtonState.Pressed)
			{
				o |= Input.Down;
			}

			if (Keyboard.GetState(P).IsKeyDown(keymapping[P][Input.Left])
				|| GamePad.GetState(P).ThumbSticks.Left.X < -.05
				|| GamePad.GetState(P).DPad.Left == ButtonState.Pressed)
			{
				o |= Input.Left;
			}
			else if (Keyboard.GetState(P).IsKeyDown(keymapping[P][Input.Right])
				|| GamePad.GetState(P).ThumbSticks.Left.X > .05
				|| GamePad.GetState(P).DPad.Right == ButtonState.Pressed)
			{
				o |= Input.Right;
			}

			if (Keyboard.GetState(P).IsKeyDown(keymapping[P][Input.FireLight])
				|| GamePad.GetState(P).Triggers.Right > 0.5)
			{
				o |= Input.FireLight;
			}

			if (Keyboard.GetState(P).IsKeyDown(keymapping[P][Input.FireHeavy])
			|| GamePad.GetState(P).Triggers.Left > 0.5)
			{
				o |= Input.FireHeavy;
			}

			return o;
		}
	}
}