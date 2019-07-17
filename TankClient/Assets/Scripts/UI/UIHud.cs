using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Tank
{
	public class UIHud : MonoBehaviour
	{
		[SerializeField] private GameObject _parentInProgress;
		[SerializeField] private GameObject _parentPause;
		[SerializeField] private GameObject _parentGameOver;


		private bool _isPaused = false;
		
		
		private void Awake()
		{
			GameUI.ListenForMessages(HandleMessage);
		}

		private void OnDestroy()
		{
			GameUI.StopListeningForMessages(HandleMessage);	
		}

		private void HandleMessage(UIMessage message)
		{
			switch (message.type)
			{
				case UIMessage.MessageType.PauseGame:
				{
					SetPaused(!_isPaused);
					_parentPause.SetActive(_isPaused);
					_parentInProgress.SetActive(!_isPaused);
				} break;

				case UIMessage.MessageType.GameOver:
				{
					SetPaused(false);
					_parentPause.SetActive(false);
					_parentInProgress.SetActive(false);
					_parentGameOver.SetActive(true);
				} break;

				case UIMessage.MessageType.QuitGame:
				{
					SetPaused(false);
				} break;
			}
		}

		private void SetPaused(bool pause)
		{
			_isPaused = pause;
			Time.timeScale = _isPaused ? 0f : 1f;
		}

	}
}
