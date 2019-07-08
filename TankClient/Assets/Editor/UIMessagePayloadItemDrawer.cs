using UnityEngine;
using UnityEditor;

namespace Glazman.Tank
{
	/// <summary>
	/// A simple custom inspector to pass data from UI widgets to our game.
	/// </summary>
	[CustomPropertyDrawer(typeof(UIMessage.Datum))]
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
			payloadType.enumValueIndex = (int)(UIMessage.Datum.Type)EditorGUI.EnumPopup(typePosition, label, (UIMessage.Datum.Type)payloadType.enumValueIndex);

			var payloadValue = property.FindPropertyRelative("value");
			switch ((UIMessage.Datum.Type)payloadType.enumValueIndex)
			{
				case UIMessage.Datum.Type.Bool:
				{
					bool boolValue;
					bool.TryParse(payloadValue.stringValue, out boolValue);
					boolValue = EditorGUI.Toggle(valuePosition, GUIContent.none, boolValue);
					payloadValue.stringValue = boolValue.ToString();
				} break;

				case UIMessage.Datum.Type.Int:
				{
					int intValue;
					int.TryParse(payloadValue.stringValue, out intValue);
					intValue = EditorGUI.IntField(valuePosition, GUIContent.none, intValue);
					payloadValue.stringValue = intValue.ToString();
				} break;
				
				case UIMessage.Datum.Type.Float:
				{
					float floatValue;
					float.TryParse(payloadValue.stringValue, out floatValue);
					floatValue = EditorGUI.FloatField(valuePosition, GUIContent.none, floatValue);
					payloadValue.stringValue = $"{floatValue:0.0000}";
				} break;
				
				case UIMessage.Datum.Type.String:
				{
					payloadValue.stringValue = EditorGUI.TextField(valuePosition, GUIContent.none, payloadValue.stringValue);
				} break;

				case UIMessage.Datum.Type.GameObject:
				case UIMessage.Datum.Type.DropdownIndex:
				case UIMessage.Datum.Type.DropdownValue:
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
