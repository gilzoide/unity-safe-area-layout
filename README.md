# Safe Area Layout
Unity GUI [layout group](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/UIAutoLayout.html#layout-groups)
that makes children respect the [Safe Area](https://docs.unity3d.com/ScriptReference/Screen-safeArea.html).
It drives children's anchors while in Play Mode and supports [`LayoutElement.ignoreLayout`](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.UI.ILayoutIgnorer.html).


## Installing
Either:

- Install via [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html)
using the following git URL:
```
https://github.com/gilzoide/unity-safe-area-layout.git
```

- Clone this repository anywhere inside your project's `Assets` or `Packages` folder.


## How to use
1. Add the [SafeAreaLayoutGroup](Runtime/SafeAreaLayoutGroup.cs)
   script anywhere in your UI hierarchy.
   Even objects with a `Canvas` are supported.
2. (optional) Select the Safe Area edges that your layout group will respect.
3. (optional) Make specific children be ignored by the layout group by adding a `LayoutElement` to them and marking `Ignore Layout`.
   Useful for background images, for example.
4. (optional) Use the [Device Simulator](https://docs.unity3d.com/Manual/device-simulator.html)
   to simulate a device with Safe Area, like the iPhone X.
   If you're using Unity 2020 or older, you need to install the [Device Simulator Package](https://docs.unity3d.com/Packages/com.unity.device-simulator@latest/index.html).
5. (optional) Preview the driven layout while in Edit Mode by hovering over the `Hover to Preview Layout` button in the `SafeAreaLayoutGroup`'s inspector.
6. Play the game (play the game, everybody play the gaaaame)
7. Enjoy 🍾