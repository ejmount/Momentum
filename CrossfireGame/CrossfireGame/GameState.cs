using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CrossfireGame
{
	abstract class GameState
	{
		protected Game1 parent;

		delegate void StateTransitionHandler(GameState dest);
		event StateTransitionHandler OnTransition;

		public GameState(Game1 parent)
		{
			this.parent = parent;
		}

		protected void Draw(GameTime time);
		protected void Update(GameTime time);



	}
}
