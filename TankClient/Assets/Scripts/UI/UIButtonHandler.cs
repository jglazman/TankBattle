using System;
using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Tank
{
	/// <summary>
	/// Attach this to any UGUI Button to pass messages from the UI to the game.
	/// </summary>
	[RequireComponent(typeof(Button))]
	public class UIButtonHandler : MonoBehaviour
	{
		[SerializeField] private UIMessage _message;
		
		public void Event_PressButton()
		{
			if (_message.type == UIMessage.MessageType.Undefined)
				throw new ArgumentException($"UIButtonHandler has an undefined message type: {Utilities.GetPathToGameObjectInScene(gameObject)}");
			
			GameUI.BroadcastMessage(_message);
		}
	}
}
