using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CrossfireGame
{
	public enum Category { Normal, Transparent, World }

	public class BodyMetadata
	{
		public readonly Box2DX.Dynamics.Body thisBody;

		public Color color = Color.White;
		public Texture2D sprite;
		public TimeSpan expiry = TimeSpan.MaxValue;
		public Category Cat = Category.Normal;

		/// <summary>
		/// Extra properties that exist only in specific objects, rather than everything.
		/// </summary>
		protected Dictionary<string, object> Properties = new Dictionary<string, object>();

		public BodyMetadata(Box2DX.Dynamics.Body B)
		{
			this.thisBody = B;
		}

		public bool HasProperty(string name)
		{
			return Properties.ContainsKey(name);
		}

		public void SetProperty<T>(string name, T obj)
		{
			Properties.Add(name, (object)obj);
		}
		public T GetProperty<T>(string name)
		{
			return (T)Properties[name];
		}
		public void UnsetProperty(string name)
		{
			Properties.Remove(name);
		}
	}
}