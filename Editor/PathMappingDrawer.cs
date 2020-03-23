using UnityEditor;
using UnityEngine;

namespace com.enemyhideout.retargeting
{
    // PathMappingDrawer
    [CustomPropertyDrawer(typeof(PathMapping))]
    public class PathMappingDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float matchTypeControlWidth = 100;
            float controlSize = (position.width - matchTypeControlWidth) * 0.5f;
            // Calculate rects
            var fromRect = new Rect(position.x, position.y, controlSize, position.height);
            var toRect = new Rect(position.x + controlSize, position.y, controlSize, position.height);
            var matchTypeRect = new Rect(position.x + controlSize*2, position.y, matchTypeControlWidth, position.height);

            EditorGUIUtility.labelWidth = 40;
            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(fromRect, property.FindPropertyRelative("fromPath"), new GUIContent("From"));
            EditorGUI.PropertyField(toRect, property.FindPropertyRelative("toPath"), new GUIContent("To"));
            EditorGUI.PropertyField(matchTypeRect, property.FindPropertyRelative("matchType"), GUIContent.none);
            EditorGUIUtility.labelWidth = 0;

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}