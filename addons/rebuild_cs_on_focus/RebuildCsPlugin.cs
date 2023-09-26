#if TOOLS
using System;
using System.Linq;
using Godot;
namespace RebuildCsOnFocus.addons.rebuild_cs_on_focus;

[Tool]
public partial class RebuildCsPlugin : EditorPlugin
{
   RebuildOnFocusUi _rebuildOnFocusUi = default!;
   Callable _buildCallable;
   Node _godotSharpEditorPlugin = default!;
   bool _rebuildEnabled;
   bool _scanning;
   readonly StringName _msBuildPanelBuildMethod = new ("BuildProject");

   public override void _EnterTree()
   {
      _godotSharpEditorPlugin = GetParent().GetChildren().First(n => n.HasMethod("BuildProjectPressed"));
      AddUiControl();
      FindEditorBuildShortcut();
      ConnectEditorSignals();
   }

   public override void _ExitTree()
   {
      _rebuildEnabled = false;
      _scanning = false;
      RemoveControlFromContainer(CustomControlContainer.Toolbar, _rebuildOnFocusUi);
      _godotSharpEditorPlugin.Set("SkipBuildBeforePlaying", false);
      _rebuildOnFocusUi.QueueFree();
   }

   void ConnectEditorSignals()
   {
      GetTree().Root.Connect(Window.SignalName.FocusEntered, new Callable(this, MethodName.RootOnFocusEntered));

      GetEditorInterface()
         .GetResourceFilesystem()
         .Connect(EditorFileSystem.SignalName.ResourcesReload, new Callable(this, MethodName.OnResourcesReload));
   }

   void AddUiControl()
   {
      var dir = GetScript().As<CSharpScript>().ResourcePath.GetBaseDir();
      _rebuildOnFocusUi = GD.Load<PackedScene>($"{dir}/rebuild_on_focus.tscn").Instantiate<RebuildOnFocusUi>();
      _rebuildOnFocusUi.Connect(RebuildOnFocusUi.SignalName.SettingChanged, new Callable(this, MethodName.SettingChanged));
      AddControlToContainer(CustomControlContainer.Toolbar, _rebuildOnFocusUi);
   }

   void OnResourcesReload(string[] resources)
   {
      if (_scanning && resources.Any(res => res.GetFile().GetExtension() == "cs"))
      {
         _buildCallable.Call();
         GD.Print("Rebuilt .NET project");
      }
   }

   void RootOnFocusEntered()
   {
      if (!_rebuildEnabled) return;
      _scanning = true;
      GetEditorInterface().GetResourceFilesystem().ScanSources();
   }

   public override void _Process(double delta)
   {
      if (!_scanning) return;
      if (GetEditorInterface().GetResourceFilesystem().IsScanning()) return;
      _scanning = false;
   }

   void SettingChanged(int setting, bool enabled)
   {
      var menuOption = (RebuildOnFocusUi.MenuOptions) setting;

      switch (menuOption)
      {
         case RebuildOnFocusUi.MenuOptions.RebuildOnFocus:
            _rebuildEnabled = enabled;
            break;
         case RebuildOnFocusUi.MenuOptions.BuildOnPlay: 
            _godotSharpEditorPlugin.Set("SkipBuildBeforePlaying", !enabled);
            break;
         default: throw new ArgumentOutOfRangeException();
      }
   }

   // Hacky way to fetch build button shortcut, since API access to build system or editor shortcuts isn't exposed
   void FindEditorBuildShortcut()
   {
      var node = new Control();
      AddControlToBottomPanel(node, "");
      var bottomBar = node.GetParent();
      RemoveControlFromBottomPanel(node);
      node.QueueFree();

      var msBuildPanel = bottomBar.GetChildren()
         .FirstOrDefault(c => c is VBoxContainer && c.HasMethod(_msBuildPanelBuildMethod));

      if (msBuildPanel != null)
      {
         _buildCallable = new Callable(msBuildPanel, _msBuildPanelBuildMethod);
      }
   }
}
#endif