using System;
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

namespace CrossfireGame
{
	class GameMenu : GameState
	{
		public GameMenu(Game1 g)
			: base(g)
		{ }

		public override void Draw(GameTime time)
		{
			parent.GraphicsDevice.Clear(Color.Black);
		}

		public override void Update(GameTime time)
		{

			if (GamePad.GetState(0).Buttons.A == ButtonState.Pressed)
			{
				this.parent.ChangeState(typeof(RootMenu));
			}
			
		}
	}
}
