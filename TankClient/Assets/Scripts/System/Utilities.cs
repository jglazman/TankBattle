using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Glazman.Tank
{
	public enum SceneName
	{
		Bootstrap,
		MainMenu,
		Game
	}
		
	public static class Utilities
	{
		public static void LoadScene(SceneName sceneName)
		{
			SceneManager.LoadScene(sceneName.ToString());
		}

		public static string GetPathToGameObjectInScene(GameObject go)
		{
			string fullName = $"/{go.name}";
			
			Transform parent = go.transform.parent;
			while (parent != null)
			{
				fullName = $"/{parent.name}{fullName}";
				parent = parent.parent;
			}
			
			return fullName;
		}
	}
}
