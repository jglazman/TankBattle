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
		// TODO: you can't change the order of these without breaking all UI buttons, because of the way Unity serializes enums
		public enum MessageType
		{
			Undefined = 0,
			
			SplashContinue,
			StartNewGame,
			GameOver,
			QuitGame,
			
			MoveUp,
			MoveDown,
			MoveLeft,
			MoveRight,
			
			LookUp,
			LookDown,
			LookLeft,
			LookRight,
			
			Shoot,
			PauseGame,
			LevelCleared,
			LoseLife
		}

		public MessageType type;
		public List<Datum> data;

		public void Add(Datum item)
		{
			if (data == null)
				data = new List<Datum>(1);
				
			data.Add(item);
		}

		public static UIMessage Create(MessageType type, params Datum[] items)
		{
			return new UIMessage {
				type = type,
				data = new List<Datum>(items)
			};
		}
		
		
		[Serializable]
		public struct Datum
		{
			public enum Type
			{
				Bool,
				Int,
				Float,
				String,
				GameObject,
				DropdownIndex,
				DropdownValue
			}

			public Type type;
			
			/// <summary>An optional value type, converted at runtime. Unused if Type is a GameObject reference.</summary>
			public string value;
			
			/// <summary>An optional GameObject reference. Unused if Type is a value type.</summary>
			public GameObject gameObject;

			
			public bool BoolValue
			{
				get
				{
					AssertType(Type.Bool, this);
					return bool.Parse(value);
				}
			}
			
			public int IntValue
			{
				get
				{
					AssertType(Type.Int, this);
					return int.Parse(value);
				}
			}

			public float FloatValue
			{
				get
				{
					AssertType(Type.Float, this);
					return float.Parse(value);
				}
			}

			public string StringValue
			{
				get
				{
					AssertType(Type.String, this);
					return value;
				}
			}

			public GameObject GameObjectValue
			{
				get
				{
					AssertType(Type.GameObject, this);
					return gameObject;
				}
			}

			public string DropdownValue
			{
				get
				{
					AssertType(Type.DropdownValue, this);
					
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
					AssertType(Type.DropdownIndex, this);
					
					if (gameObject == null)
						return -1;
					
					var dropdown = gameObject.GetComponent<Dropdown>();
					if (dropdown == null)
						return -1;

					return dropdown.value;
				}
			}

			private static void AssertType(Type type, Datum item) =>
				Assert.IsTrue(type == item.type, $"InvalidTypeException: PayloadItem has type={item.type}, but was references as {type}");
		}
	}
}
