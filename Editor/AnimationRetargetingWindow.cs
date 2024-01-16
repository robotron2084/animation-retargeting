using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif


namespace com.enemyhideout.retargeting
{

    public class AnimationRetargetingWindow : EditorWindow
    {
#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [SerializeField]
        private StyleSheet m_Stylesheet = default;
#endif

        [SerializeField]
        List<AnimationRetargetingData> retargetingOptions;

        [SerializeField]
        int retargetingIndex;

        [SerializeField]
        List<UnityEngine.Object> items = new List<UnityEngine.Object>();

        [SerializeField]
        AnimationRetargetingData targetingData = null;

        SerializedObject windowSO = null;
        SerializedObject targetingDataSO = null;

#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
        #region GUI references
        VisualElement no_presets = null;
        VisualElement presets_present = null;

        DropdownField dd_presets = null;
        ListView lv_items = null;

        VisualElement preset_help_box = null;

        VisualElement retarget_properties = null;
        VisualElement attribute_mappings_help_box = null;
        ListView lv_attribute_mappings = null;
        ListView lv_path_mappings = null;
        #endregion

        List<Type> acceptable_draggable_types = new List<Type>()
        {
            typeof(AnimationClip),
            typeof(AnimatorController),
            typeof(UnityEditor.DefaultAsset)
        };
#else
        [SerializeField]
        string[] retargetingOptionsLabels;

        Vector2 scrollPos;

        [SerializeField]
        public float number = 10.0f;
#endif

#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
        [MenuItem("Window/Animation/Retargeting (UIT)")]
#else
        [MenuItem("Window/Animation/Retargeting")]
#endif
        static void Init()
        {
            AnimationRetargetingWindow window = GetWindow<AnimationRetargetingWindow>(new Type[]
            {
            typeof(SceneView)
            });
#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
            window.titleContent = new GUIContent("Retargeting (UIT)");
#else
            window.titleContent = new GUIContent("Retargeting");
#endif
            window.Show();
        }

        private void OnEnable()
        {
            windowSO = new SerializedObject(this);
#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
            retargetingOptions = new List<AnimationRetargetingData>();
#else
            updateRetargetingOptions();
            updateRetargetingIndex();
#endif
        }

#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
        private void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;


            if (m_VisualTreeAsset == null)
            {
                m_VisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.enemyhideout.retargeting/Editor/UIToolkit/AnimationRetargetingWindow.uxml");
            }
            m_VisualTreeAsset.CloneTree(root);
            if (m_Stylesheet == null)
            {
                m_Stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.enemyhideout.retargeting/Editor/UIToolkit/AnimationRetargetingWindow.uss");
            }
            root.styleSheets.Add(m_Stylesheet);

            no_presets = root.Q<VisualElement>("no-presets");
            presets_present = root.Q<VisualElement>("presets-present");
            bool has_options = retargetingOptions.Count > 0;
            SwitchLayout(has_options, no_presets, presets_present);

            {//No Presets
                Button btn_create_first = no_presets.Q<Button>("create-first");
                btn_create_first.clickable.clicked += NewPreset;
            }

            {//Presets present
                dd_presets = presets_present.Q<DropdownField>("preset-selection");
                dd_presets.RegisterValueChangedCallback<string>(OnPresetSelected);

                {
                    Button btn_copy_preset = presets_present.Q<Button>("copy-preset");
                    btn_copy_preset.clickable.clicked += CopyPreset;
                }
                {
                    Button btn_new_preset = presets_present.Q<Button>("new-preset");
                    btn_new_preset.clickable.clicked += NewPreset;
                }
                {
                    lv_items = presets_present.Q<ListView>("items");
                    Foldout header_foldout = lv_items.Q<Foldout>();
                    header_foldout.value = false;
                    lv_items.Bind(windowSO);

                    lv_items.RegisterCallback<DragUpdatedEvent>(OnItemsDragUpdated);
                }

                preset_help_box = presets_present.Q<VisualElement>("preset-help-box");

                retarget_properties = presets_present.Q<VisualElement>("retarget-properties");
                RebindRetargetProperties(null, false);

                attribute_mappings_help_box = retarget_properties.Q<VisualElement>("attribute-mappings-help-box");

                {
                    lv_attribute_mappings = presets_present.Q<ListView>("attribute-mappings-list");
                    Foldout header_foldout = lv_attribute_mappings.Q<Foldout>();
                    header_foldout.value = false;

                    lv_attribute_mappings.itemsAdded += OnAttributeMappingsAdded;
                    lv_attribute_mappings.itemsRemoved += OnAttributeMappingsRemoved;
                }

                {
                    lv_path_mappings = presets_present.Q<ListView>("path-mappings-list");
                    Foldout header_foldout = lv_path_mappings.Q<Foldout>();
                    header_foldout.value = false;
                }

                {
                    Button btn_retarget = presets_present.Q<Button>("retarget");
                    btn_retarget.clickable.clicked += Retarget;
                }
            }
            OnFocus();
        }

        private void OnAttributeMappingsAdded(IEnumerable<int> obj)
        {
            SetAttributeMappingsHelpBoxState(obj.Count());
        }

        private void OnAttributeMappingsRemoved(IEnumerable<int> obj)
        {
            SetAttributeMappingsHelpBoxState(-obj.Count());
        }

        void SetAttributeMappingsHelpBoxState(int next_count = 0)
        {
            if (lv_attribute_mappings == null
                || lv_attribute_mappings.itemsSource == null
                || attribute_mappings_help_box == null)
            {
                return;
            }
            if ((lv_attribute_mappings.itemsSource.Count + next_count) > 0
                || next_count > 0)
            {
                attribute_mappings_help_box.style.visibility = Visibility.Hidden;
                attribute_mappings_help_box.style.display = DisplayStyle.None;
                attribute_mappings_help_box.style.overflow = Overflow.Hidden;
            }
            else
            {
                attribute_mappings_help_box.style.visibility = Visibility.Visible;
                attribute_mappings_help_box.style.display = DisplayStyle.Flex;
                attribute_mappings_help_box.style.overflow = Overflow.Visible;
            }
        }

        private void OnItemsDragUpdated(DragUpdatedEvent evt)
        {
            if (evt.currentTarget != lv_items) return;

            List<UnityEngine.Object> accepted_draggables = new List<UnityEngine.Object>();
            foreach (UnityEngine.Object draggable in DragAndDrop.objectReferences)
            {
                if (acceptable_draggable_types.Contains(draggable.GetType()))
                {
                    if (draggable.GetType() == typeof(UnityEditor.DefaultAsset))
                    {
                        string path = AssetDatabase.GetAssetPath(draggable);
                        //Check if it's a folder
                        bool is_folder = AssetDatabase.IsValidFolder(path);
                        if (is_folder)
                        {
                            accepted_draggables.Add(draggable);
                        }
                    }
                    else
                    {
                        accepted_draggables.Add(draggable);
                    }
                }
            }

            if (accepted_draggables.Count > 0)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }

            evt.StopPropagation();
        }

        private void OnPresetSelected(ChangeEvent<string> evt)
        {
            DropdownField dd = (DropdownField)evt.target;
            if (evt.newValue != evt.previousValue)
            {
                retargetingIndex = dd.index;
                if (retargetingIndex > -1)
                {
                    targetingData = retargetingOptions[retargetingIndex];
                    targetingDataSO = new SerializedObject(targetingData);

                    RebindRetargetProperties(targetingDataSO, !targetingData.isPreset);
                }
                else
                {
                    RebindRetargetProperties(null, false);
                }
            }
        }

        void SwitchLayout(bool has_options, VisualElement no_presets, VisualElement presets_present)
        {
            if (no_presets == null
                || presets_present == null)
                return;

            if (has_options)
            {
                no_presets.SetEnabled(false);
                no_presets.style.visibility = Visibility.Hidden;
                no_presets.style.display = DisplayStyle.None;
                no_presets.style.overflow = Overflow.Hidden;

                presets_present.SetEnabled(true);
                presets_present.style.visibility = Visibility.Visible;
                presets_present.style.display = DisplayStyle.Flex;
                presets_present.style.overflow = Overflow.Visible;
            }
            else
            {
                presets_present.SetEnabled(false);
                presets_present.style.visibility = Visibility.Hidden;
                presets_present.style.display = DisplayStyle.None;
                presets_present.style.overflow = Overflow.Hidden;

                no_presets.SetEnabled(true);
                no_presets.style.visibility = Visibility.Visible;
                no_presets.style.display = DisplayStyle.Flex;
                no_presets.style.overflow = Overflow.Visible;
            }
        }

        void RebindRetargetProperties(SerializedObject new_so, bool enabled = true)
        {
            if (enabled)
            {
                preset_help_box.style.visibility = Visibility.Hidden;
                preset_help_box.style.display = DisplayStyle.None;
                preset_help_box.style.overflow = Overflow.Hidden;
            }
            else
            {
                preset_help_box.style.visibility = Visibility.Visible;
                preset_help_box.style.display = DisplayStyle.Flex;
                preset_help_box.style.overflow = Overflow.Visible;
            }

            retarget_properties.SetEnabled(false);
            if (new_so != null)
            {
                retarget_properties.Unbind();
                retarget_properties.Bind(new_so);
                SetAttributeMappingsHelpBoxState();
            }
            else
            {
                retarget_properties.Unbind();
            }
            retarget_properties.SetEnabled(enabled);
        }
#else
        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            windowSO.Update();

            if (retargetingOptions.Count == 0)
            {
                // display help
                EditorGUILayout.HelpBox("In order to retarget animations, you need to create an Animation Retargeting scriptable object or select one of the ones that comes with the project.", MessageType.Info);
                if (GUILayout.Button("Create Animation Retargeting Data"))
                {
                    createInstance<AnimationRetargetingData>("Animation Retargeting", ref targetingData);
                    updateRetargetingOptions();
                    updateRetargetingIndex();
                }
            }
            else
            {
                // the interface should show up as normal.
                EditorGUILayout.BeginHorizontal();
                if (retargetingIndex >= retargetingOptions.Count)
                {
                    retargetingIndex = retargetingOptions.Count - 1;
                    targetingData = retargetingOptions[retargetingIndex];
                }
                int newIndex = EditorGUILayout.Popup("Select An Option", retargetingIndex, retargetingOptionsLabels);
                if (newIndex != retargetingIndex)
                {
                    retargetingIndex = newIndex;
                    targetingDataSO = null;
                }                
                if(retargetingIndex > -1)
                {
                    targetingData = retargetingOptions[retargetingIndex];
                }
                if (targetingData != null)
                {
                    if (GUILayout.Button("Copy", GUILayout.Width(100)))
                    {
                        CopyPreset();
                    }
                }
                if (GUILayout.Button("New", GUILayout.Width(100)))
                {
                    NewPreset();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("Drag Animation Clips, Animators, or Folders into the Items list below to retarget all clips inside them.", MessageType.Info);

                SerializedProperty itemsProp = windowSO.FindProperty("items");
                EditorGUILayout.PropertyField(itemsProp, new GUIContent("Items"), true);

                if (targetingData != null)
                {
                    if (targetingDataSO == null)
                    {
                        targetingDataSO = new SerializedObject(targetingData);
                    }
                    targetingDataSO.Update();

                    if (targetingData.isPreset)
                    {
                        EditorGUILayout.HelpBox("'" + targetingData.name + "' is a preset and cannot be modified. In order to edit it, make a copy first.", MessageType.Info);
                        GUI.enabled = false;
                    }

                    SerializedProperty inputPrefix = targetingDataSO.FindProperty("inputPrefix");
                    EditorGUILayout.PropertyField(inputPrefix, new GUIContent("Input Prefix"));
                    SerializedProperty outputPrefix = targetingDataSO.FindProperty("outputPrefix");
                    EditorGUILayout.PropertyField(outputPrefix, new GUIContent("Output Prefix"));

                    SerializedProperty attributeMappings = targetingDataSO.FindProperty("attributeMappings");
                    if (targetingData.attributeMappings.Count == 0)
                    {
                        EditorGUILayout.HelpBox("You need to add an 'attribute mapping' to retarget your animation properties from one path to another. This is the name of the property you are animating.", MessageType.Info);
                    }
                    EditorGUILayout.PropertyField(attributeMappings, new GUIContent("Attribute Mappings"), true);

                    SerializedProperty pathMappings = targetingDataSO.FindProperty("pathMappings");
                    EditorGUILayout.PropertyField(pathMappings, new GUIContent("Path Mappings"), true);


                    if (targetingData.isPreset)
                    {
                        GUI.enabled = true;
                    }

                    targetingDataSO.ApplyModifiedProperties();

                    if (GUILayout.Button("Retarget"))
                    {
                        Retarget();
                    }
                }
                windowSO.ApplyModifiedProperties();
            }
            EditorGUILayout.EndScrollView();
        }
#endif

        private void Retarget()
        {
            AttributeMapper remapper = new AttributeMapper();
            List<AnimationClip> clips = getClipsFromItems(items);

            remapper.Retarget(targetingData, clips);
        }

        private void OnFocus()
        {
            updateRetargetingOptions();
            updateRetargetingIndex();

#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT

            if (targetingData != null
            && retarget_properties != null)
            {
                retarget_properties.SetEnabled(!targetingData.isPreset);
            }
#endif
        }

        void updateRetargetingOptions()
        {
            retargetingOptions = new List<AnimationRetargetingData>();
            string[] guids = AssetDatabase.FindAssets("t:AnimationRetargetingData");

#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
            List<string> retargetingOptionsLabels = new List<string>();
#else
            retargetingOptionsLabels = new string[guids.Length];
#endif
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                AnimationRetargetingData option = (AnimationRetargetingData)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationRetargetingData));
                retargetingOptions.Add(option);

#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
                retargetingOptionsLabels.Add(option.name);
#else
                retargetingOptionsLabels[i] = option.name;
#endif
            }

#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
            SwitchLayout(retargetingOptions.Count > 0, no_presets, presets_present);

            if (dd_presets == null) return;
            dd_presets.choices = retargetingOptionsLabels;
#endif
        }

        void updateRetargetingIndex()
        {
#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
            if (dd_presets == null) return;

            if (dd_presets.choices.Count == 0)
            {
                retargetingIndex = -1;
                dd_presets.value = "";
                dd_presets.index = -1;
            }
            else
            {
#endif
                if (targetingData == null)
                {
                    retargetingIndex = 0;
                }

                retargetingIndex = retargetingOptions.IndexOf(targetingData);

#if ANIMATION_RETARGETING_UTILITY_UITOOLKIT
                dd_presets.index = retargetingIndex;
            }
#endif
        }

        void NewPreset()
        {
            createInstance<AnimationRetargetingData>("Animation Retargeting", ref targetingData);
            updateRetargetingOptions();
            updateRetargetingIndex();
        }

        void CopyPreset()
        {
            if (targetingData == null)
            {
                return;
            }
            string copyPath = generatePathName(targetingData.name);
            AnimationRetargetingData copy = Instantiate(targetingData);
            copy.isPreset = false;
            saveAsset(copyPath, copy);
            copy.name = Path.GetFileNameWithoutExtension(copyPath);
            targetingData = copy;
            updateRetargetingOptions();
            updateRetargetingIndex();
        }

        void createInstance<T>(string defaultName, ref T value) where T : ScriptableObject
        {
            string path = generatePathName(defaultName);
            value = ScriptableObject.CreateInstance<T>();
            saveAsset(path, value);
        }

        string generatePathName(string defaultName)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + defaultName + ".asset");
            return assetPathAndName;

        }

        void saveAsset(string assetPathAndName, UnityEngine.Object obj)
        {
            AssetDatabase.CreateAsset(obj, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
        }

        List<AnimationClip> getClipsFromItems(List<UnityEngine.Object> items)
        {
            List<AnimationClip> retVal = new List<AnimationClip>();
            foreach (UnityEngine.Object item in items)
            {
                if (item != null)
                {
                    if (item is AnimationClip)
                    {
                        retVal.Add((AnimationClip)item);
                    }
                    else
                    if (item is AnimatorController)
                    {
                        Debug.Log("[AnimationRetargetingWindow] Found AnimatorController.");
                        AnimatorController ac = (AnimatorController)item;
                        retVal.AddRange(ac.animationClips);
                    }
                    else
                    if (item is UnityEditor.DefaultAsset)
                    {
                        string path = AssetDatabase.GetAssetPath(item);
                        if (Directory.Exists(path))
                        {
                            string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new string[] { path });
                            foreach (string guid in guids)
                            {
                                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                                AnimationClip clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationClip));
                                retVal.Add(clip);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[AnimationRetargetingWindow] WARNING! " + item + " is not a clip, animator, or Folder!");
                    }
                }
            }
            return retVal.Distinct().ToList();
        }
    }
}