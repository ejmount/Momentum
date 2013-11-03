using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CrossfireGame
{
	class RootMenu : GameState
	{
		public RootMenu(Game1 g)
			: base(g) 
		{ }


		public override void Draw(GameTime time)
		{
			parent.GraphicsDevice.Clear(Color.BurlyWood);
			
		}

		public override void Update(GameTime time)
		{
			if (GamePad.GetState(0).Buttons.B == ButtonState.Pressed)
			{
				this.parent.ChangeState(typeof(GameMenu));
			}
		}
	}
}
