using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace com.enemyhideout.retargeting
{
    public class AttributeMapper
    {
        static List<String> baseProperties = new List<string>{
          "m_FloatCurves",
          "m_EditorCurves"
        };

        StringBuilder logger;

        public void Retarget(AnimationRetargetingData targetingData, List<AnimationClip> selectedClips)
        {
          initLogger();
          foreach(AnimationClip clip in selectedClips)
          {
            log("[AttributeMapper] Retargeting clip:"  + clip);
            SerializedObject so = new SerializedObject(clip);

            foreach(AttributeMapping mapping in targetingData.attributeMappings)
            {
              List<SerializedProperty> props = propertiesFor(so, targetingData.inputPrefix + "." + mapping.fromPath, baseProperties);
              if(props.Count == 0)
              {
                log("[AttributeMapper] did not find property:"  +mapping.fromPath);
              }else{

                switch(mapping.action)
                {
                  case AttributeMappingAction.Replace:
                    log("[AttributeMapper] Replacing "+mapping.fromPath +" to " + mapping.toPath);
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
            so.ApplyModifiedProperties();
          }
          flushLog();
        }

        List<SerializedProperty> propertiesFor(SerializedObject so, string propSearch, List<string> baseProperties)
        {
          List<SerializedProperty> retVal = new List<SerializedProperty>();

          foreach(string baseProperty in baseProperties)
          {
            SerializedProperty curves = so.FindProperty(baseProperty);
            SerializedProperty endSentinel = curves.Copy();
            endSentinel.Next(false);
            // Log("[AttributeMapper] end:" + endSentinel.propertyPath);
            curves.Next(true);
            curves.Next(true);
            while(curves.Next(false) && !SerializedProperty.EqualContents(curves, endSentinel))
            {
              // Log("[AttributeMapper] prop:" + curves.propertyPath + " " + curves.propertyType);
              SerializedProperty child = curves.FindPropertyRelative("attribute");

              if(child != null)
              {
                // Log("[AttributeMapper] child:" + child.propertyPath + " " + child.stringValue);
                if(child.stringValue == propSearch)
                {
                  retVal.Add(child);
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