using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Glazman.Tank
{
	[CustomPropertyDrawer(typeof(UIMessage.PayloadItem))]
	public class UIMessagePayloadItemDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			EditorGUI.indentLevel++;

			var typePosition = position;
			typePosition.width *= 0.6f;

			var valuePosition = position;
			valuePosition.width *= 0.4f;
			valuePosition.x += typePosition.width;
			
			var payloadType = property.FindPropertyRelative("type");
			payloadType.enumValueIndex = (int)(UIMessage.PayloadItem.PayloadType)EditorGUI.EnumPopup(typePosition, label, (UIMessage.PayloadItem.PayloadType)payloadType.enumValueIndex);

			var payloadValue = property.FindPropertyRelative("value");
			switch ((UIMessage.PayloadItem.PayloadType)payloadType.enumValueIndex)
			{
				case UIMessage.PayloadItem.PayloadType.Bool:
				{
					bool boolValue;
					bool.TryParse(payloadValue.stringValue, out boolValue);
					boolValue = EditorGUI.Toggle(valuePosition, GUIContent.none, boolValue);
					payloadValue.stringValue = boolValue.ToString();
				} break;

				case UIMessage.PayloadItem.PayloadType.Int:
				{
					int intValue;
					int.TryParse(payloadValue.stringValue, out intValue);
					intValue = EditorGUI.IntField(valuePosition, GUIContent.none, intValue);
					payloadValue.stringValue = intValue.ToString();
				} break;
				
				case UIMessage.PayloadItem.PayloadType.Float:
				{
					float floatValue;
					float.TryParse(payloadValue.stringValue, out floatValue);
					floatValue = EditorGUI.FloatField(valuePosition, GUIContent.none, floatValue);
					payloadValue.stringValue = $"{floatValue:0.0000}";
				} break;
				
				case UIMessage.PayloadItem.PayloadType.String:
				{
					payloadValue.stringValue = EditorGUI.TextField(valuePosition, GUIContent.none, payloadValue.stringValue);
				} break;

				case UIMessage.PayloadItem.PayloadType.GameObject:
				case UIMessage.PayloadItem.PayloadType.DropdownIndex:
				case UIMessage.PayloadItem.PayloadType.DropdownValue:
				{
					var goValue = property.FindPropertyRelative("gameObject");
					EditorGUI.ObjectField(valuePosition, goValue, GUIContent.none);
				} break;

				default:
				{
					EditorGUI.LabelField(valuePosition, "unsupported");
				} break;
			}
			
			EditorGUI.indentLevel--;			
			EditorGUI.EndProperty();
		}
	}
}
