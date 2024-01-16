using System;

namespace com.enemyhideout.retargeting
{

    public enum AttributeMappingAction
    {
      Rename,
      Copy,
      Delete
    }

    public enum MatchType
    {
        ExactMatch,
        Contains
    }

    [Serializable]
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

    [Serializable]
    public class PathMapping
    {
        public string fromPath;
        public string toPath;
        public MatchType matchType;

        public override string ToString()
        {
            return "[PathMapping fromPath="+fromPath +", toPath="+toPath+"]";
        }
    }
}