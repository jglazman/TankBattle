using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Glazman.Tank
{
	public class UserInput : UnityBehaviour
	{
		public override int Priority { get { return ModulePriority.UserInput; } }

		private static Dictionary<KeyCode, UIMessage> KeyBindings = new Dictionary<KeyCode, UIMessage>
		{
			{ KeyCode.W, UIMessage.Create(UIMessage.MessageType.MoveUp) },
			{ KeyCode.A, UIMessage.Create(UIMessage.MessageType.MoveLeft) },
			{ KeyCode.S, UIMessage.Create(UIMessage.MessageType.MoveDown) },
			{ KeyCode.D, UIMessage.Create(UIMessage.MessageType.MoveRight) },
			
			{ KeyCode.UpArrow, UIMessage.Create(UIMessage.MessageType.LookUp) },
			{ KeyCode.LeftArrow, UIMessage.Create(UIMessage.MessageType.LookLeft) },
			{ KeyCode.DownArrow, UIMessage.Create(UIMessage.MessageType.LookDown) },
			{ KeyCode.RightArrow, UIMessage.Create(UIMessage.MessageType.LookRight) },
		};

		private static Dictionary<KeyCode, UIMessage> KeyDownBindings = new Dictionary<KeyCode, UIMessage>
		{
			{ KeyCode.Space, UIMessage.Create(UIMessage.MessageType.Shoot) }
		};
		
		private KeyCode[] _keys;	// minor optimization, because we iterate over this every frame
		private KeyCode[] _keysDown;

		public UserInput()
		{
			_keys = KeyBindings.Keys.ToArray();
			_keysDown = KeyDownBindings.Keys.ToArray();
		}
		
		public override void Update(float deltaTime)
		{
			for (int i = 0; i < _keysDown.Length; i++)
			{
				if (Input.GetKeyDown(_keysDown[i]))
					GameUI.BroadcastMessage(KeyDownBindings[_keysDown[i]]);
			}
			
			for (int i = 0; i < _keys.Length; i++)
			{
				if (Input.GetKey(_keys[i]))
					GameUI.BroadcastMessage(KeyBindings[_keys[i]]);
			}
		}
	}
}
