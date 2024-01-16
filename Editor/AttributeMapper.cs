using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Collections.Generic;

namespace com.enemyhideout.retargeting
{
    public class AttributeMapper
    {
        static List<String> baseProperties = new List<string>{
          "m_FloatCurves",
          "m_EditorCurves"
        };

        // I could probably do a regex and a full property crawl, but considering the 
        // size of animation data that could exist I think this is a worthwhile optimization.
        static List<String> pathProperties = new List<string>{
          "m_FloatCurves",
          "m_EditorCurves",
          "m_RotationCurves",
          "m_PositionCurves",
          "m_ScaleCurves"

          /**
          List of likely suspects taken from yaml:
          m_RotationCurves
          m_CompressedRotationCurves
          m_EulerCurves
          m_PositionCurves
          m_ScaleCurves
          m_FloatCurves
          m_PPtrCurves
          m_EditorCurves
          m_EulerEditorCurves
          */
        };

        const string attributePropName = "attribute";
        const string pathPropName = "path";

        StringBuilder logger;

        public void Retarget(AnimationRetargetingData targetingData, List<AnimationClip> selectedClips)
        {
          initLogger();
          foreach(AnimationClip clip in selectedClips)
          {
            log("[AttributeMapper] Retargeting clip:"  + clip);
            SerializedObject so = new SerializedObject(clip);
            RemapAttributes(targetingData, so);
            RemapPaths(targetingData, so);
            
            so.ApplyModifiedProperties();
          }
          flushLog();
        }


        void RemapAttributes(AnimationRetargetingData targetingData, SerializedObject so)
        {
          foreach(AttributeMapping mapping in targetingData.attributeMappings)
          {
            List<SerializedProperty> props = propertiesFor(so, attributePropName, targetingData.inputPrefix + "." + mapping.fromPath, MatchType.ExactMatch, baseProperties);
            if(props.Count == 0)
            {
              log("[AttributeMapper] did not find property:"  +mapping.fromPath);
            }else{

              switch(mapping.action)
              {
                case AttributeMappingAction.Rename:
                  log("[AttributeMapper] Renaming "+mapping.fromPath +" to " + mapping.toPath);
                  foreach(SerializedProperty prop in props)
                  {
                    // log("[AttributeMapper] found property...modifying."+ prop.propertyPath);
                    prop.stringValue = targetingData.outputPrefix + "." + mapping.toPath;
                  }
                  
                break;
                case AttributeMappingAction.Copy:
                  foreach(SerializedProperty prop in props)
                  {
                    string initialPath = prop.propertyPath;
                    SerializedProperty parentProp = parentOf(prop);
                    int index;
                    if(tryGetIndexOf(initialPath, out index))
                    { 
                      parentProp.InsertArrayElementAtIndex(index);
                      SerializedProperty dupeProp = parentProp.GetArrayElementAtIndex(index+1).FindPropertyRelative("attribute");

                      // log("[AttributeMapper.COPY] found property...modifying."+ dupeProp.propertyPath);
                      dupeProp.stringValue = targetingData.outputPrefix + "." + mapping.toPath;
                    }else{
                      log("[AttributeMapper] did not find index....");
                    }
                  }
                break;
                case AttributeMappingAction.Delete:
                foreach(SerializedProperty prop in props)
                {
                  string initialPath = prop.propertyPath;
                  SerializedProperty parentProp = parentOf(prop);
                  int index;
                  if(tryGetIndexOf(initialPath, out index))
                  {
                    parentProp.DeleteArrayElementAtIndex(index);
                  }else{
                    log("[AttributeMapper] did not find index....");
                  }
                }
                break;
                
              }
            }
          }
        }

        void RemapPaths(AnimationRetargetingData targetingData, SerializedObject so)
        {
          foreach(PathMapping mapping in targetingData.pathMappings)
          {
            List<SerializedProperty> props = propertiesFor(so, pathPropName, mapping.fromPath, mapping.matchType, pathProperties);
            if(props.Count == 0)
            {
              log("[AttributeMapper] did not find property for path:"  +mapping.fromPath);
            }else{
              foreach(SerializedProperty prop in props)
              {
                if(mapping.matchType == MatchType.ExactMatch)
                {
                log("[AttributeMapper] Renaming "+mapping.fromPath +" to " + mapping.toPath );
                  prop.stringValue = mapping.toPath;
                }else if(mapping.matchType == MatchType.Contains)
                {
                  string newVal = prop.stringValue.Replace(mapping.fromPath, mapping.toPath);
                  log("[AttributeMapper] Renaming "+ prop.stringValue +" to " + newVal );
                  prop.stringValue = newVal;
                }
              }
            }
          }
        }



        List<SerializedProperty> propertiesFor(SerializedObject so, string propName, string propValue, MatchType matchType, List<string> baseProperties)
        {
          List<SerializedProperty> retVal = new List<SerializedProperty>();

          foreach(string baseProperty in baseProperties)
          {
            SerializedProperty curves = so.FindProperty(baseProperty);
            SerializedProperty endSentinel = curves.Copy();
            endSentinel.Next(false);
            // Log("[AttributeMapper] end:" + 
            curves.Next(true);
            curves.Next(true);
            while(curves.Next(false) && !SerializedProperty.EqualContents(curves, endSentinel))
            {
              // Log("[AttributeMapper] prop:" + curves.propertyPath + " " + curves.propertyType);
              SerializedProperty child = curves.FindPropertyRelative(propName);

              if(child != null)
              {
                if(matchType == MatchType.ExactMatch)
                {
                  // Log("[AttributeMapper] child:" + child.propertyPath + " " + child.stringValue);
                  if(child.stringValue == propValue)
                  {
                    retVal.Add(child);
                  }
                }else if(matchType == MatchType.Contains)
                {
                  if(child.stringValue.IndexOf(propValue) > -1)
                  {
                    retVal.Add(child);
                  }
                }
              }else{
                // log("[AttributeMapper] nope");
              }
            }
          }
          return retVal;
        }

        bool tryGetIndexOf(string propPath, out int val)
        {
          // this is so dumb. There is no 'index of' for SerializedProperties.
          int startIndex = propPath.IndexOf("[") + 1;
          int endIndex = propPath.IndexOf("]");

          int index = -1;
          string token = propPath.Substring(startIndex, endIndex - startIndex);
          if(Int32.TryParse(token, out index))
          {
            val = index;
            return true;
          }
          val = index;
          return false;
        }

        SerializedProperty parentOf(SerializedProperty prop)
        {
          string propPath = prop.propertyPath.Substring(0, prop.propertyPath.IndexOf("."));
          SerializedProperty parentProp = prop.serializedObject.FindProperty(propPath);
          return parentProp;
          
        }

        void log(string message)
        {
          logger.AppendLine(message);
        }

        void flushLog()
        {
          string log = logger.ToString();
          if(log.Length > 0)
          {
            Debug.Log(logger);
          }
          initLogger();
          logger = new StringBuilder();
        }

        void initLogger()
        {
          logger = new StringBuilder();
        }
    }
}