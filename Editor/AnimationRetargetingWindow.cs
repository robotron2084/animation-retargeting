using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace com.enemyhideout.retargeting
{

  public class AnimationRetargetingWindow : EditorWindow
  {
      [SerializeField]
      List<AnimationRetargetingData> retargetingOptions;

      [SerializeField]
      int retargetingIndex;

      [SerializeField]
      string[] retargetingOptionsLabels;

      [SerializeField]
      AnimationRetargetingData targetingData;
      Vector2 scrollPos;

      [SerializeField]
      List<UnityEngine.Object> items = new List<UnityEngine.Object>();


      [SerializeField]
      public float number = 10.0f;

      [MenuItem("Window/Animation/Retargeting")]
      static void Init()
      {
          AnimationRetargetingWindow window = (AnimationRetargetingWindow)EditorWindow.GetWindow(typeof(AnimationRetargetingWindow), false, "Retargeting");
          window.Show();
      }

      void OnFocus()
      {
        updateRetargetingOptions();
      }

      void OnGUI()
      {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginHorizontal();
        if(retargetingIndex >= retargetingOptions.Count)
        {
          retargetingIndex = retargetingOptions.Count-1;
          targetingData = retargetingOptions[retargetingIndex];
        }
        int newIndex = EditorGUILayout.Popup("Select An Option", retargetingIndex, retargetingOptionsLabels);
        if(newIndex != retargetingIndex)
        {
          retargetingIndex = newIndex;
        }
        targetingData = retargetingOptions[retargetingIndex];
        if(targetingData != null)
        {
          if(GUILayout.Button("Copy", GUILayout.Width(100)))
          {
            string copyPath = generatePathName(targetingData.name);
            AnimationRetargetingData copy = Instantiate(targetingData);
            copy.isPreset = false;
            saveAsset(copyPath, copy);
            copy.name = Path.GetFileNameWithoutExtension(copyPath);
            targetingData = copy;
            updateRetargetingOptions();
            updateRetargetingIndex();
          }
        }
        if(GUILayout.Button("New", GUILayout.Width(100)))
        {
          createInstance<AnimationRetargetingData>("Animation Retargeting", ref targetingData);
          updateRetargetingOptions();
          updateRetargetingIndex();
        }

        EditorGUILayout.EndHorizontal();
        // targetingData = (AnimationRetargetingData)EditorGUILayout.ObjectField(targetingData, typeof(AnimationRetargetingData), false);
        if(targetingData != null)
        {
          SerializedObject so = new SerializedObject(targetingData);
          SerializedObject windowSO = new SerializedObject(this);
          SerializedProperty attributeMappings = so.FindProperty("attributeMappings");
          EditorGUILayout.HelpBox("Drag Animation Clips, Animators, or Folders into the Items list below to retarget all clips inside them.", MessageType.Info);
          SerializedProperty itemsProp = windowSO.FindProperty("items");
          EditorGUILayout.PropertyField(itemsProp, new GUIContent("Items"), true);

          if(targetingData.isPreset)
          {
            EditorGUILayout.HelpBox( "'" + targetingData.name + "' is a preset and cannot be modified. In order to edit it, make a copy first.", MessageType.Info);
            GUI.enabled = false;
          }
          SerializedProperty inputPrefix = so.FindProperty("inputPrefix");
          EditorGUILayout.PropertyField(inputPrefix, new GUIContent("Input Prefix"));
          SerializedProperty outputPrefix = so.FindProperty("outputPrefix");
          EditorGUILayout.PropertyField(outputPrefix, new GUIContent("Output Prefix"));
          if(targetingData.attributeMappings.Count == 0)
          {
            EditorGUILayout.HelpBox("You need to add an 'attribute mapping' to retarget your animation properties from one path to another. This is the name of the property you are animating.", MessageType.Info);
          }
          EditorGUILayout.PropertyField(attributeMappings, new GUIContent("Attribute Mappings"), true);
          if(targetingData.isPreset)
          {
            GUI.enabled = true;
          }

          so.ApplyModifiedProperties();
          windowSO.ApplyModifiedProperties();

          if(GUILayout.Button("Retarget"))
          {
            AttributeMapper remapper = new AttributeMapper();
            List<AnimationClip> clips = getClipsFromItems(items);
            
            remapper.Retarget(targetingData, clips);
          }
        }else{
          if(retargetingOptions.Count == 0)
          {
            // display help
            EditorGUILayout.HelpBox("In order to retarget animations, you need to create an Animation Retargeting scriptable object or select one of the ones that comes with the project.", MessageType.Info);
            if(GUILayout.Button("Create Animation Retargeting Data" ))
            {
              createInstance<AnimationRetargetingData>("Animation Retargeting", ref targetingData);
            }
          }else{
            // the interface should show up as normal.
          }
        }

        EditorGUILayout.EndScrollView();
      }

      void createInstance<T>(string defaultName, ref T value) where T : ScriptableObject
      {
        string path = generatePathName(defaultName);
        value = ScriptableObject.CreateInstance<T>();
        saveAsset(path, value);
      }

      string generatePathName(string defaultName)
      {
        string path = AssetDatabase.GetAssetPath (Selection.activeObject);
        if (path == "") 
        {
          path = "Assets";
        } 
        else if (Path.GetExtension (path) != "") 
        {
          path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
        }
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/"+defaultName+".asset");
        return assetPathAndName;

      }

      void saveAsset(string assetPathAndName, UnityEngine.Object obj)
      {
        AssetDatabase.CreateAsset (obj, assetPathAndName);
        AssetDatabase.SaveAssets ();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow ();
      }

      List<AnimationClip> getClipsFromItems(List<UnityEngine.Object> items)
      {
        List<AnimationClip> retVal = new List<AnimationClip>();
        foreach(UnityEngine.Object item in items)
        {
          if(item != null)
          {
            if(item is AnimationClip)
            {
              retVal.Add((AnimationClip) item);
            }else
            if(item is AnimatorController)
            {
              Debug.Log("[AnimationRetargetingWindow] Found AnimatorController.");
              AnimatorController ac = (AnimatorController)item;
              retVal.AddRange(ac.animationClips);
            }else
            if(item is UnityEditor.DefaultAsset)
            {
              string path = AssetDatabase.GetAssetPath(item);
              if(Directory.Exists(path))
              {
                string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new string[]{ path });
                foreach(string guid in guids)
                {
                  string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                  AnimationClip clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationClip));
                  retVal.Add(clip);
                }
              }
            }
            else{
              Debug.LogWarning("[AnimationRetargetingWindow] WARNING! " + item + " is not a clip, animator, or Folder!");
            }
          }
        }
        return retVal.Distinct().ToList();
      }

      void updateRetargetingOptions()
      {
        retargetingOptions = new List<AnimationRetargetingData>();
        string[] guids = AssetDatabase.FindAssets("t:AnimationRetargetingData");
        retargetingOptionsLabels = new string[guids.Length+1];
        for(int i=0; i < guids.Length; i++ )
        {
          string guid = guids[i];
          string assetPath = AssetDatabase.GUIDToAssetPath(guid);
          AnimationRetargetingData option = (AnimationRetargetingData)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationRetargetingData));
          retargetingOptions.Add(option);
          retargetingOptionsLabels[i] = option.name;
        }
      }

      void updateRetargetingIndex()
      {
        if(targetingData == null)
        {
          retargetingIndex = 0;
        }
        retargetingIndex = retargetingOptions.IndexOf(targetingData);
      }
  }
}