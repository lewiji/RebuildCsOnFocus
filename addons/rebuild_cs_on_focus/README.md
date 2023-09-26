Rebuild C# Project On Focus plugin for Godot 4.x
===

Mimics Unity style behaviour, where if changes to scripts are detected when the editor window is 
refocused (i.e. due to tabbing in and out of an IDE), the .NET build process is automatically triggered.
Optionally, the "build on run" functionality of the editor can be disabled.

Since the C# `BuildManager` and related `GodotTools` classes aren't exposed to scripting, this takes 
a somewhat hacky approach of grabbing the nodes from the bottom bar, and using the Godot API to 
identify if any of them has a `BuildProject` method (indicating it's of (unexposed) type 
`GodotTools.Build.MSBuildPanel`) and creating a `Callable` out of it to trigger the build process.

Feasibly this could be written in gdscript, but since it will be used in .NET projects anyway, it's 
in C#. Since this functionality is usually desired for `[Tool]` scripts, using C# it should be possible
to add a preference to only rebuild if changes to `[Tool]` scripts are detected via reflection on the
`ToolAnnotation` type. However, for now there is just a simple on/off checkbox added to the top toolbar.

To use: add the `addons/rebuild_cs_on_focus` folder to your project, build the solution, and then enable 
the plugin in Project Settings. Rebuild options can be toggled from the "Rebuild" menu button in the
editor toolbar.
