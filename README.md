# Animation Retargeting

This is an editor window for the Unity game engine that allows for the retargeting of animations from different paths (aka game objects) and properties(aka keyframes on those objects). The main focus is to allow for the conversion of different items such as blend shapes between different common formats such as Apple's FaceCap and Mixamo's rigs. Allows for:
 * Renaming one property to another (for example from `browInnerUp` to `BrowsOuterLower_Left`
 * Deleting unnecessary properties (for example if a rig does not support it).
 * Copying one attribute over to a new one (for example to duplicate keys to right and left from a rig that only supports one side).

![Screenshot](screenshot.png) 
 
# Installation
You can install this editor through [Unity's package manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@1.8/manual/index.html). 

In 2019 you can install the latest from this github by [following these instructions](https://docs.unity3d.com/Manual/upm-ui-giturl.html). 
Alternatively you can add the following to your 'dependencies' in your Packages/manifest.json file:

```
    "com.enemyhideout.retargeting": "https://github.com/robotron2084/animation-retargeting.git",
```

# Usage



Find this utility helpful? Perhaps you will also find my Unity asset [Project Search & Replace](https://assetstore.unity.com/packages/tools/utilities/project-search-replace-55680) helpful.
