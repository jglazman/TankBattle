using UnityEngine;

namespace Glazman.Tank
{
	public class Bootstrap : MonoBehaviour
	{
		// this is the entry point to our application
		private void Awake()
		{
			Game.Initialize();
		}
	}
	
	// this is an alternative to a MonoBehaviour-based entry point, but i think a MonoBehaviour is more explicit
//	public class Bootstrap
//	{
//		[RuntimeInitializeOnLoadMethod]
//		private static void Main()
//		{
//			Game.Initialize();
//		}
//	}

}
