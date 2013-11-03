using Microsoft.Xna.Framework;

namespace CrossfireGame
{
	public abstract class GameState
	{
		protected Game1 parent;

		public GameState(Game1 parent)
		{
			this.parent = parent;
		}

		public abstract void Draw(GameTime time);

		public abstract void Update(GameTime time);
	}
}