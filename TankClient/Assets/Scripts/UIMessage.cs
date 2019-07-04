using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace Glazman.Tank
{
	[Serializable]
	public class UIMessage
	{
		public enum MessageType
		{
			Undefined = 0,
			SplashContinue,
			StartNewGame,
			PauseGame,
			QuitGame
		}

		[Serializable]
		public struct PayloadItem
		{
			public enum PayloadType
			{
				Bool,
				Int,
				Float,
				String,
				GameObject,
				DropdownIndex,
				DropdownValue
			}

			public PayloadType type;
			public string value;
			public GameObject gameObject;

			public bool BoolValue
			{
				get
				{
					// no dropdown support
					AssertType(PayloadType.Bool, this);
					return bool.Parse(value);
				}
			}
			
			public int IntValue
			{
				get
				{
					AssertType(PayloadType.Int, this);
					return int.Parse(value);
				}
			}

			public float FloatValue
			{
				get
				{
					AssertType(PayloadType.Float, this);
					return float.Parse(value);
				}
			}

			public string StringValue
			{
				get
				{
					AssertType(PayloadType.String, this);
					return value;
				}
			}

			public GameObject GameObjectValue
			{
				get
				{
					// no dropdown support
					AssertType(PayloadType.GameObject, this);
					return gameObject;
				}
			}

			public string DropdownValue
			{
				get
				{
					AssertType(PayloadType.DropdownValue, this);
					
					if (gameObject == null)
						return "";
					
					var dropdown = gameObject.GetComponent<Dropdown>();
					if (dropdown == null)
						return "";

					return dropdown.options[dropdown.value].text;
				}
			}

			public int DropdownIndex
			{
				get
				{
					AssertType(PayloadType.DropdownIndex, this);
					
					if (gameObject == null)
						return -1;
					
					var dropdown = gameObject.GetComponent<Dropdown>();
					if (dropdown == null)
						return -1;

					return dropdown.value;
				}
			}

			private static void AssertType(PayloadType type, PayloadItem item) =>
				Assert.IsTrue(type == item.type, $"InvalidTypeException: PayloadItem has type={item.type}, but was references as {type}");
		}

		public MessageType type;
		public List<PayloadItem> items;

		public void Add(PayloadItem item)
		{
			if (items == null)
				items = new List<PayloadItem>(1);
				
			items.Add(item);
		}

		public static UIMessage Create(MessageType type, params PayloadItem[] items)
		{
			return new UIMessage {
				type = type,
				items = new List<PayloadItem>(items)
			};
		}
	}
}
