using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Glazman.Tank
{
	/// <summary>
	/// Attach this to any UGUI Button to pass messages from the UI to the game.
	/// </summary>
	[RequireComponent(typeof(Button))]
	public class UIButtonHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[Tooltip("This is the message to be broadcast when this button is activated.")]
		[SerializeField] private UIMessage _message;

		[Header("Options")]
		
		[Tooltip("If enabled, then the event callback will be broadcast once when the button is pressed.")]
		[SerializeField] private bool _broadcastOnClick;
		
		[Tooltip("If enabled, then the event callback will be broadcast each frame as long as this button is being held down.")]
		[SerializeField] private bool _broadcastWhilePressed;
		
		[Tooltip("If enabled, then the button hitbox will exactly match the target graphic sprite (sprite must have read/write enabled).")]
		[SerializeField] private bool _matchHitboxToSprite;
		
		[Tooltip("If enabled, then our button visual states will activate if a matching keyboard shortcut is used.")]
		[SerializeField] private bool _listenToKeyBindings;
		
		private bool _isHover;
		private UIButton _button;
		private Image _buttonImage;
		private bool _isKeyActive;
		
		private void Awake()
		{
			_button = GetComponent<UIButton>();

			if (_button != null)
			{
				_buttonImage = _button.targetGraphic as Image;
				if (_buttonImage != null)
					_buttonImage.alphaHitTestMinimumThreshold = _matchHitboxToSprite ? 1f : 0f;
			}
			
			if (_listenToKeyBindings)
				GameUI.ListenForMessages(HandleUIMessage);
		}

		private void OnDestroy()
		{
			if (_listenToKeyBindings)
				GameUI.StopListeningForMessages(HandleUIMessage);
		}
		
		private void HandleUIMessage(UIMessage message)
		{
			if (message.type == _message.type)
				_isKeyActive = true;
		}

		private void Update()
		{
			bool isPressed = Input.GetKey(KeyCode.Mouse0);

			// set visual state
			if (_button != null)
			{
				if (_isKeyActive)
				{
					_button.SetStatePressed();
					_isKeyActive = false;
				}
				else if (_isHover)
				{
					if (isPressed)
						_button.SetStatePressed();
					else
						_button.SetStateHighlighted();
				}
				else
				{
					_button.SetStateNormal();
				}
			}

			// check activation rules
			if (_isHover && isPressed && _broadcastWhilePressed)
				BroadcastMessage();
		}
		
		public void OnPointerClick(PointerEventData eventData)
		{
			if (_broadcastOnClick)
				BroadcastMessage();
		}
		
		public void OnPointerEnter(PointerEventData eventData)
		{
			_isHover = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_isHover = false;
		}


		private void BroadcastMessage()
		{
			if (_message.type == UIMessage.MessageType.Undefined)
				throw new ArgumentException($"UIButtonHandler has an undefined message type: {Utilities.GetPathToGameObjectInScene(gameObject)}");
			
			GameUI.BroadcastMessage(_message);
		}
	}
}
