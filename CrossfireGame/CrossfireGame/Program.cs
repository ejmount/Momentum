using System.IO;
using System.Linq;
namespace CrossfireGame
{
#if WINDOWS || XBOX

	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		private static void Main(string[] args)
		{
#if WINDOWS 
			try {
#endif
			using (Game1 game = new Game1())
			{
				game.Run();
			}
#if WINDOWS
			}
			catch(System.Exception e)
			{
				var numFiles = new DirectoryInfo( System.Environment.CurrentDirectory).EnumerateDirectories().Count();
				using (StreamWriter sw = new StreamWriter( "output" + numFiles.ToString("D4") + ".txt"))
				{
					sw.WriteLine(e.Message);
					sw.WriteLine(e.StackTrace);
				}
			}
#endif
		}
	}

#endif
}