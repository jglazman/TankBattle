

namespace Glazman.Tank
{
	/// <summary>
	/// A simple UI message router.
	/// </summary>
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
