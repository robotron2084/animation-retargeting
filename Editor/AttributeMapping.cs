using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace com.enemyhideout.retargeting
{

    public enum AttributeMappingAction
    {
      Rename,
      Copy,
      Delete
    }

    [System.Serializable]
    public class AttributeMapping
    {
        public string fromPath;
        public AttributeMappingAction action;
        public string toPath;

        public override string ToString()
        {
            return "[AttributeMapping fromPath="+fromPath+", action="+action+", toPath="+toPath+"]";
        }
    }
}