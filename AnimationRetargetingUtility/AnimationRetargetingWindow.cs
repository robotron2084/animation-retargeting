using UnityEngine;
using UnityEditor;

namespace com.enemyhideout.retargeting
{

  public class AnimationRetargetingWindow : EditorWindow
  {

      AnimationRetargetingData targetingData;
      Vector2 scrollPos;

      [MenuItem("Window/Animation/Retargeting")]
      static void Init()
      {
          AnimationRetargetingWindow window = (AnimationRetargetingWindow)EditorWindow.GetWindow(typeof(AnimationRetargetingWindow), false, "Retargeting");
          window.Show();
      }

      void OnGUI()
      {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos); 
        targetingData = (AnimationRetargetingData)EditorGUILayout.ObjectField(targetingData, typeof(AnimationRetargetingData), false);
        if(targetingData != null)
        {
          SerializedObject so = new SerializedObject(targetingData);
          SerializedProperty clips = so.FindProperty("selectedClips");
          SerializedProperty attributeMappings = so.FindProperty("attributeMappings");
          EditorGUILayout.PropertyField(clips, new GUIContent("Clips"), true);
          SerializedProperty inputPrefix = so.FindProperty("inputPrefix");
          EditorGUILayout.PropertyField(inputPrefix, new GUIContent("Input Prefix"));
          SerializedProperty outputPrefix = so.FindProperty("outputPrefix");
          EditorGUILayout.PropertyField(outputPrefix, new GUIContent("Output Prefix"));
          EditorGUILayout.PropertyField(attributeMappings, new GUIContent("Attribute Mappings"), true);
          so.ApplyModifiedProperties();

          if(GUILayout.Button("Retarget"))
          {
            Debug.Log("[AnimationRetargetingWindow] Doing retargeting.");
            AttributeMapper remapper = new AttributeMapper();
            remapper.Retarget(targetingData);
          }
        }

        EditorGUILayout.EndScrollView();
      }
  }
}