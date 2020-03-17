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

First open the utility via `Window->Animation->Retargeting`

Upon first open, the window will prompt you that you need to create an Animation Retargeting scriptable object. Press the button to do so. Your settings for retargeting will be saved to this asset.

Next you must specify which Animation Clips you would like to modify. You can do this by dragging the animations into the `Clips` UI.

Lastly you must specify a number of 'attribute mapping' actions. These are the properties that you'd like to modify, and how you'd like to modify them. There are three parameters to a mapping:
  * `From Path` - The path, or property that you'd like to modify. For example if you'd like to modify the `Position` of a transform input `Position`.
  * `To Path` - The new name of the property.
  * `Action` - This can be either `Copy`, `Replace`, or `Delete`.
    * `Copy` - Make a copy of this property with the `To Path` name.
    * `Replace` - Rename the property to the `To Path`'s name.
    * `Delete` - Delete this property.

Once you have input your clips and mappings, then press the `Retarget` button, and the tool will iterate over your animation clips and update them. And that's it!

## Retargeting Presets
  You can create as many retargeting presets as you need and drag them into the interface to use them. To create more presets create a new `Animation Retargeting Data` from the `Create` menu.

Find this utility helpful? Perhaps you will also find my Unity asset [Project Search & Replace](https://assetstore.unity.com/packages/tools/utilities/project-search-replace-55680) helpful.
