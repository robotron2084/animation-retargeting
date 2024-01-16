
using UnityEditor;
using UnityEngine;
#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace com.enemyhideout.retargeting
{
    // AttributeMappingDrawer
    [CustomPropertyDrawer(typeof(AttributeMapping))]
    public class AttributeMappingDrawer : PropertyDrawer
    {
#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            property.serializedObject.Update();

            SerializedProperty actionProp = property.FindPropertyRelative("action");
            SerializedProperty fromProp = property.FindPropertyRelative("fromPath");
            SerializedProperty toProp = property.FindPropertyRelative("toPath");

            AttributeMappingAction action_type = (AttributeMappingAction)actionProp.intValue;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            TextField fromField = new TextField("From");
            fromField.BindProperty(fromProp);
            fromField.style.flexGrow = 1;
            fromField.style.flexShrink = 1; 
            fromField.style.flexBasis = new StyleLength(StyleKeyword.Auto);
            fromField.style.width = new StyleLength(new Length(45.0f, LengthUnit.Percent)); 
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
            toField.style.flexShrink = 1;
            toField.style.flexBasis = new StyleLength(StyleKeyword.Auto);
            toField.style.width = new StyleLength(new Length(45.0f, LengthUnit.Percent)); 
            toField.style.marginLeft = 3;
            toField.style.marginRight = 3;
            toField.style.marginTop = 1;
            toField.style.marginBottom = 1;
            toField.style.height = new StyleLength(new Length(100.0f, LengthUnit.Percent));
            container.Add(toField);

            PropertyField actionField = new PropertyField(actionProp, "");
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

            SetToVisibility(container, action_type);
            actionField.RegisterValueChangeCallback((e) =>
            {
                AttributeMappingAction action_type = (AttributeMappingAction)e.changedProperty.intValue;
                SetToVisibility(container, action_type);
            });

            return container;
        }

        void SetToVisibility(VisualElement container, AttributeMappingAction action_type)
        {
            TextField toField = container.Q<TextField>("toField");
            if (action_type == AttributeMappingAction.Delete
                && toField != null)
            {
                toField.style.visibility = Visibility.Hidden;
                toField.style.display = DisplayStyle.None;
                toField.style.overflow = Overflow.Hidden;
            }
            else
            {
                toField.style.visibility = Visibility.Visible;
                toField.style.display = DisplayStyle.Flex;
                toField.style.overflow = Overflow.Visible;
            }
        }
#else
        // Draw the property inside the given rect
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

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }
#endif
    }
}