using UnityEngine;
using UnityEditor;
using System.IO;

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
          if(targetingData.selectedClips.Count == 0)
          {
            EditorGUILayout.HelpBox("Drag animation clips into the 'Clips' array below to retarget them.", MessageType.Info);
          }
          EditorGUILayout.PropertyField(clips, new GUIContent("Clips"), true);

          SerializedProperty inputPrefix = so.FindProperty("inputPrefix");
          EditorGUILayout.PropertyField(inputPrefix, new GUIContent("Input Prefix"));
          SerializedProperty outputPrefix = so.FindProperty("outputPrefix");
          EditorGUILayout.PropertyField(outputPrefix, new GUIContent("Output Prefix"));
          if(targetingData.attributeMappings.Count == 0)
          {
            EditorGUILayout.HelpBox("You need to add an 'attribute mapping' to retarget your animation properties from one path to another. This is the name of the property you are animating.", MessageType.Info);
          }
          EditorGUILayout.PropertyField(attributeMappings, new GUIContent("Attribute Mappings"), true);
          so.ApplyModifiedProperties();

          if(GUILayout.Button("Retarget"))
          {
            Debug.Log("[AnimationRetargetingWindow] Doing retargeting.");
            AttributeMapper remapper = new AttributeMapper();
            remapper.Retarget(targetingData);
          }
        }else{
          // display help
          EditorGUILayout.HelpBox("In order to retarget animations, you need to create an Animation Retargeting scriptable object or select one of the ones that comes with the project.", MessageType.Info);
          string path = AssetDatabase.GetAssetPath (Selection.activeObject);
          if (path == "") 
          {
            path = "Assets";
          } 
          else if (Path.GetExtension (path) != "") 
          {
            path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
          }

          if(GUILayout.Button("Create Animation Retargeting Data in " + path))
          {
            targetingData = ScriptableObject.CreateInstance<AnimationRetargetingData>();
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/AnimationRetargeting.asset");
             
            AssetDatabase.CreateAsset (targetingData, assetPathAndName);
         
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow ();
          }
        }

        EditorGUILayout.EndScrollView();
      }
  }
}