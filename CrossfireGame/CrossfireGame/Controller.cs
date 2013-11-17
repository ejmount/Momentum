using System;

namespace CrossfireGame
{
	internal class Controller
	{
		[Flags]
		private enum Input
		{
			None,
			Up, Down,
			Left, Right,
			FireHeavy,
			FireLight
		}

		private static Input InterpretInput(int playerIndex = 0)
		{
		}
	}
}