using UnityEditor;
using UnityEngine;

namespace com.enemyhideout.retargeting
{
    // AttributeMappingDrawer
    [CustomPropertyDrawer(typeof(AttributeMapping))]
    public class AttributeMappingDrawer : PropertyDrawer
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

            SerializedProperty actionProp = property.FindPropertyRelative("action");
            float actionControlWidth = 100;
            bool isDeleting = (AttributeMappingAction)actionProp.intValue == AttributeMappingAction.Delete;
            float controlSize = isDeleting ? position.width - actionControlWidth : (position.width - actionControlWidth) * 0.5f;
            // Calculate rects
            var fromRect = new Rect(position.x, position.y, controlSize, position.height);
            var toRect = new Rect(position.x + controlSize, position.y, controlSize, position.height);
            var actionRect = new Rect(position.x + controlSize*2, position.y, actionControlWidth, position.height);
            if(isDeleting)
            {
                actionRect.x = position.x + controlSize;
            }

            EditorGUIUtility.labelWidth = 40;
            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(fromRect, property.FindPropertyRelative("fromPath"), new GUIContent("From"));
            if(!isDeleting)
            {
                EditorGUI.PropertyField(toRect, property.FindPropertyRelative("toPath"), new GUIContent("To"));
            }
            EditorGUI.PropertyField(actionRect, actionProp, GUIContent.none);
            EditorGUIUtility.labelWidth = 0;

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}