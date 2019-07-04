using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Glazman.Tank
{
	[RequireComponent(typeof(Button))]
	public class UIButtonHandler : MonoBehaviour
	{
		[SerializeField] private UIMessage _message;
		
		public void Event_PressButton()
		{
			Assert.IsTrue(_message.type != UIMessage.MessageType.Undefined,
				$"UIButtonHandler has an undefined message type: {Utilities.GetPathToGameObjectInScene(gameObject)}");

			GameUI.BroadcastMessage(_message);
		}
	}
}
