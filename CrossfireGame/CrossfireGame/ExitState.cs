﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossfireGame
{
	/// <summary>
	/// Because this means I don't have to special-case the menu code. 
	/// </summary>
	[MenuItem("Exit", int.MaxValue)]
	class ExitState : GameState
	{

		public ExitState(Game1 g) : base(g) { }

		public override void Draw(Microsoft.Xna.Framework.GameTime time)
		{
			
		}

		public override void Update(Microsoft.Xna.Framework.GameTime time)
		{
			this.parent.Exit();
		}
	}
}
