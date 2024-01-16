using UnityEditor;
using UnityEngine;
#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace com.enemyhideout.retargeting
{
    // PathMappingDrawer
    [CustomPropertyDrawer(typeof(PathMapping))]
    public class PathMappingDrawer : PropertyDrawer
    {
#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            property.serializedObject.Update();

            SerializedProperty matchTypeProp = property.FindPropertyRelative("matchType");
            SerializedProperty fromProp = property.FindPropertyRelative("fromPath");
            SerializedProperty toProp = property.FindPropertyRelative("toPath");

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            TextField fromField = new TextField("From");
            fromField.BindProperty(fromProp);
            fromField.style.flexGrow = 1;
            fromField.style.flexBasis = new StyleLength(StyleKeyword.Auto);
            fromField.style.marginLeft = 3;
            fromField.style.marginRight = 3;
            fromField.style.marginTop = 1;
            fromField.style.marginBottom = 1;
            fromField.style.height = new StyleLength(new Length(100.0f, LengthUnit.Percent));
            container.Add(fromField);

            TextField toField = new TextField("To");
            toField.BindProperty(toProp);
            toField.name = "toField";
            toField.style.flexGrow = 1;
            toField.style.flexBasis = new StyleLength(StyleKeyword.Auto);
            toField.style.marginLeft = 3;
            toField.style.marginRight = 3;
            toField.style.marginTop = 1;
            toField.style.marginBottom = 1;
            toField.style.height = new StyleLength(new Length(100.0f, LengthUnit.Percent)); 
            container.Add(toField);

            PropertyField actionField = new PropertyField(matchTypeProp, "");
            actionField.style.flexBasis = new StyleLength(StyleKeyword.Auto);
            actionField.style.width = new StyleLength(new Length(100.0f, LengthUnit.Pixel));
            actionField.style.height = new StyleLength(new Length(100.0f, LengthUnit.Percent));
            actionField.style.marginLeft = 3;
            actionField.style.marginRight = 3;
            container.Add(actionField);

            fromField.schedule.Execute(() =>
            {
                Label l = fromField.Q<Label>();
                l.style.flexShrink = 1;
                l.style.minWidth = 0;
                l.style.width = new StyleLength(new Length(40, LengthUnit.Pixel));
            }).ExecuteLater(2);
            toField.schedule.Execute(() =>
            {
                Label l = toField.Q<Label>();
                l.style.flexShrink = 1;
                l.style.minWidth = 0;
                l.style.width = new StyleLength(new Length(40, LengthUnit.Pixel));
            }).ExecuteLater(2);

            return container;
        }
#else
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            property.serializedObject.Update();

            EditorGUI.BeginChangeCheck();

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

            if(EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }
#endif
    }
}