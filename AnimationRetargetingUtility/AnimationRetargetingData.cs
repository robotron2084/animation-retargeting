using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace com.enemyhideout.retargeting
{
    [CreateAssetMenu(fileName = "Retargeting", menuName = "Animation Retargeting Data")]
    public class AnimationRetargetingData : ScriptableObject
    {
        public List<AnimationClip> selectedClips;
        public string inputPrefix = "";
        public string outputPrefix = "";
        public List<AttributeMapping> attributeMappings;


    }
}