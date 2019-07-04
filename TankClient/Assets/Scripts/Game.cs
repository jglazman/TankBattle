using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Tank
{
	public enum GameState
	{
		Bootstrap,
		MainMenu,
		GameInProgress,
		GameOver
	}

	public enum Difficulty
	{
		Easy = 0,
		Normal,
		Hard
	}
	
	public static class Game
	{
		private static GameState _gameState;
		
		
		public static void Initialize()
		{
//			Assert.raiseExceptions = true;
			
			GameUI.ListenForMessages(HandleUIMessage);
		}

		private static void HandleUIMessage(UIMessage message)
		{
			switch (message.type)
			{
				case UIMessage.MessageType.SplashContinue:
				{
					Assert.IsTrue(_gameState == GameState.Bootstrap);
					
					_gameState = GameState.MainMenu;
					Utilities.LoadScene(SceneName.MainMenu);
				} break;
				
				case UIMessage.MessageType.StartNewGame:
				{
					Assert.IsTrue(_gameState == GameState.MainMenu);
					
					var difficulty = (Difficulty)message.items[0].DropdownIndex;
					StartNewGame(difficulty);
				} break;
			}
		}

		private static void StartNewGame(Difficulty difficulty)
		{
			Debug.LogWarning($"Start new game!  Difficulty={difficulty}");

			_gameState = GameState.GameInProgress;
			Utilities.LoadScene(SceneName.Game);
		}
	}
}
