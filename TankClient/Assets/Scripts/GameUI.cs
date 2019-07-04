using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Tank
{
	public static class GameUI
	{
		public delegate void ReceiveMessageDelegate(UIMessage message);

		private static event ReceiveMessageDelegate _messageListeners;

		public static void ListenForMessages(ReceiveMessageDelegate listener)
		{
			_messageListeners += listener;
		}

		public static void StopListeningForMessages(ReceiveMessageDelegate listener)
		{
			_messageListeners -= listener;
		}
		
		public static void BroadcastMessage(UIMessage message)
		{
			_messageListeners?.Invoke(message);
		}
	}
}
